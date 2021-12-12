use tokio_postgres::{Error, NoTls,Client};
use serde::{Serialize, Deserialize};
use warp::Filter;
use parking_lot::{Mutex, RwLock};
use std::sync::{Arc};
use std::collections::HashMap;
use blake2::{Blake2b, Digest};
use std::time::{Duration, Instant};
use tokio::time::sleep;
use std::net::IpAddr;
use std::fs::File;

#[derive(Clone)]
struct Platform {
    id: i32,
    name: String
}

#[derive(Copy, Clone)]
struct Image {
    id: i32,
    hash64: u64,
    hash256: [u64; 4]
}

trait Compare {
    type T;
    fn compare(&self, other: Self::T) -> u32;
}

impl Compare for u64 {
    type T = u64;
    fn compare(&self, other: u64) -> u32 {
        return (self ^ other).count_ones();
    }
}

impl Compare for [u64; 4] {
    type T = [u64; 4];
    fn compare(&self, other: [u64; 4]) -> u32 {
        let mut mismatch_count = 0;

        for i in 0..4 {
            mismatch_count += self[i].compare(other[i])
        }

        return mismatch_count;
    }
}

#[derive(Serialize)]
struct ComparedImage {
    id: i32,
    mismatch_count: u32
}

struct ImageCollection {
    platform: Platform,
    change_id: Mutex<i64>,
    number_of_shards: usize,
    sfw_shards: RwLock<Vec<Vec<Image>>>,
    nsfw_shards: RwLock<Vec<Vec<Image>>>
}

impl ImageCollection {
    const BATCH_SIZE: i64 = 25000;
    const THRESHOLD: u32 = 18;

    fn new(platform: Platform, number_of_shards: usize) -> ImageCollection {
        return ImageCollection {
            platform: platform,
            change_id: Mutex::new(0),
            number_of_shards: number_of_shards,
            sfw_shards: ImageCollection::generate_shards(number_of_shards),
            nsfw_shards: ImageCollection::generate_shards(number_of_shards)
        };
    }

    fn generate_shards(number_of_shards: usize) -> RwLock<Vec<Vec<Image>>> {
        let mut shards: Vec<Vec<Image>> = Vec::with_capacity(number_of_shards);
        for _ in 0..number_of_shards {
            shards.push(Vec::new());
        }

        return RwLock::new(shards);
    }

    fn calculate_shard_index(&self, id: i32) -> usize {
        let mut hasher = Blake2b::new();
        hasher.update(id.to_le_bytes());
        let hash = hasher.finalize();
        let shard: usize = u16::from_le_bytes([hash[0], hash[1]]).into();

        return shard % self.number_of_shards;
    }

    fn add(&self, image: Image, is_sfw: bool) {
        let shard = self.calculate_shard_index(image.id);
        let lock = self.get_shard(is_sfw);
        let mut shards = lock.write();
        
        shards[shard].push(image);
    }

    fn remove(&self, id: i32) {
        let index = self.calculate_shard_index(id);

        self.remove_from_shard(id, true, index);
        self.remove_from_shard(id, false, index);
    }

    fn remove_from_shard(&self, id: i32, is_sfw: bool, index: usize) {
        let mut shards = self.get_shard(is_sfw).write();

        shards[index].retain(|&i| i.id != id);
    }

    fn get_shard(&self, is_sfw: bool) -> &RwLock<Vec<Vec<Image>>> {
        return if is_sfw { &self.sfw_shards } else { &self.nsfw_shards };
    }

    fn compare(&self, hash64: u64, hash256: [u64; 4], include_nsfw: bool) -> (usize, Vec<ComparedImage>) {
        let mut results = self._compare(&self.sfw_shards, hash64, hash256);

        if !include_nsfw {
            return results;
        }

        let mut nsfw_results = self._compare(&self.nsfw_shards, hash64, hash256);
        results.0 += nsfw_results.0;
        results.1.append(&mut nsfw_results.1);

        return results;
    }

    fn _compare(&self, shards: &RwLock<Vec<Vec<Image>>>, hash64: u64, hash256: [u64; 4]) -> (usize, Vec<ComparedImage>) {
        let mut matches: Vec<ComparedImage> = Vec::new();
        
        let mut count: usize = 0;
        let shards = shards.read();
        for shard in shards.iter() {
            for image in shard {
                let mismatch_count = image.hash64.compare(hash64);

                if mismatch_count <= ImageCollection::THRESHOLD  {
                    let mismatch_count = image.hash256.compare(hash256);
                    matches.push(ComparedImage { id: image.id, mismatch_count: mismatch_count })
                }
            }

            count += shard.len();
        }

        return (count, matches);
    }
    
    async fn refresh(&self, client: &Client) -> Result<(), Error> {
        loop {
            let get_change_id = || {
                let lock = self.change_id.lock();
                return *lock;
            };

            let set_change_id = |value: i64| {
                let mut lock = self.change_id.lock();
                *lock = value;
            };

            let results = client.query("
                SELECT id, is_sfw, change_id, is_deleted, phash_average64, phash_average256
                FROM denormalized_image
                WHERE platform_id = $1 AND change_id > $2
                ORDER BY change_id
                LIMIT $3", &[&self.platform.id, &get_change_id(), &ImageCollection::BATCH_SIZE]).await?;
            
            let progress = format!("{} - Applying {} changes...", self.platform.name, results.len());
            for row in results.iter() {
                let id: i32 = row.get(0);
                let is_sfw: bool = row.get(1);
                let change_id: i64 = row.get(2);
                let is_deleted: bool = row.get(3);
                let phash_average64: Vec<u8> = row.get(4);
                let phash_average256: Vec<u8> = row.get(5);

                fn bytes_to_u64(slice: &[u8]) -> u64 {
                    return u64::from_le_bytes([
                        slice[0],
                        slice[1],
                        slice[2],
                        slice[3],
                        slice[4],
                        slice[5],
                        slice[6],
                        slice[7]
                    ]);
                }

                if is_deleted {
                    self.remove(id);
                } else {
                    let phash_average64 = bytes_to_u64(&phash_average64);
                    let phash_average256 = [
                        bytes_to_u64(&phash_average256[0..8]),
                        bytes_to_u64(&phash_average256[8..16]),
                        bytes_to_u64(&phash_average256[16..24]),
                        bytes_to_u64(&phash_average256[24..32])
                    ];

                    self.add(Image { id: id, hash64: phash_average64, hash256: phash_average256 }, is_sfw);
                }

                if change_id > get_change_id() {
                    set_change_id(change_id);
                }
            }

            println!("{} changes applied till change ID {}", progress, get_change_id());

            if (results.len() as i64) < ImageCollection::BATCH_SIZE {
                self.shrink_shards(&self.sfw_shards);
                self.shrink_shards(&self.nsfw_shards);

                break;
            }
        }

        Ok(())
    }

    fn shrink_shards(&self, shards: &RwLock<Vec<Vec<Image>>>) {
        let mut map = shards.write();

        for images in map.iter_mut() {
            images.shrink_to_fit();
        }
    }
}

struct ImageService {
    collections: HashMap<i32, ImageCollection>
}

impl ImageService {
    async fn new(client: &Client) -> Result<ImageService, Error> {
        let mut collections = HashMap::new();
        for row in client.query("SELECT id, name FROM platform", &[]).await? {
            let id: i32 = row.get(0);
            let name: String = row.get(1);
            let platform = Platform {
                id: id,
                name: name
            };

            collections.insert(platform.id, ImageCollection::new(platform, 5000));
        }

        return Ok(ImageService {
            collections: collections
        })
    }

    async fn refresh(&self, client: &Client) -> Result<(), Error> {
        for (_, collection) in self.collections.iter() {
            collection.refresh(&client).await?;
        }
        
        Ok(())
    }

    fn compare(&self, hash64: u64, hash256: [u64; 4], include_nsfw: bool, limit: usize) -> HashMap<i32, CompareResult> {
        let mut results: HashMap<i32, CompareResult> = HashMap::new();
        for (platform_id, collection) in self.collections.iter() {
            let (collection_count, mut collection_results) = collection.compare(hash64, hash256, include_nsfw);
            collection_results.sort_by(|a, b| a.mismatch_count.cmp(&b.mismatch_count));
            let collection_results = collection_results.into_iter().take(limit).collect::<Vec<ComparedImage>>();

            results.insert(platform_id.clone(), CompareResult {
                count: collection_count,
                images: collection_results
            });
        }

        return results;
    }
}

#[derive(Deserialize)]
struct Config {
    host: IpAddr,
    port: u16,
    refresh_interval: u64,
    db: DatabaseConfig
}

#[derive(Deserialize)]
struct DatabaseConfig {
    host: String,
    user: String,
    password: String,
    name: String
}

fn read_config(config_location: &str) -> Config {
    let file = File::open(config_location).unwrap();
    let config = serde_yaml::from_reader::<File, Config>(file).unwrap();

    return config;
}

#[tokio::main]
async fn main() -> Result<(), Error> {
    let config = read_config("config.yml");
    
    let connection_string = format!("host={} user={} password={} dbname={}", config.db.host, config.db.user, config.db.password, config.db.name);
    let (client, connection) = tokio_postgres::connect(&connection_string, NoTls).await?;
    tokio::spawn(async move {
        if let Err(e) = connection.await {
            eprintln!("Connection error: {}", e);
        }
    });

    let service = ImageService::new(&client).await?;
    service.refresh(&client).await?;
    let service = Arc::new(service);

    let warp_service = service.clone();
    tokio::spawn(async move {
        let compare = warp::path!("compare" / u64 / u64 / u64 / u64 / u64 / bool / usize)
        .map(move |hash64, hash256p1, hash256p2, hash256p3, hash256p4, include_nsfw, limit| -> warp::reply::Json {
            let duration = Instant::now();
            let result = warp_service.compare(hash64, [hash256p1, hash256p2, hash256p3, hash256p4], include_nsfw, limit);
            
            let mut count = 0;
            for platform_count in result.iter().map(|x| x.1.count) {
                count += platform_count;
            }
            println!("Compared {} images in {} ms", count, duration.elapsed().as_millis());
            
            return warp::reply::json(&result);
        });

        warp::serve(compare)
            .run((config.host, config.port))
            .await;
    });

    loop {
        sleep(Duration::from_secs(config.refresh_interval)).await;
        service.refresh(&client).await?;
    }
}

#[derive(Serialize)]
struct CompareResult {
    count: usize,
    images: Vec<ComparedImage>
}
