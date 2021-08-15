import * as React from 'react'
import { SearchResult, SearchResultItem } from '../services/api';
import GalleryThumbnail from './gallery-thumbnail';
import Icon from './icon';

const GalleryCard = ({ data }: { data: SearchResultItem }) => {
    return (
        <a className="square w-1/2 sm:w-1/3 md:w-1/4 p-1 block relative" href={data.location} target="_blank" rel="noreferrer">
            <div className="absolute left-0 top-0 w-full h-full p-inherit">
                <div className="relative w-full h-full rounded overflow-hidden">
                    <div className={`absolute top-0 left-0 w-7 p-0.5 bg-gradient-${data.match.class} rounded-tl rounded-br z-10`}>
                        <Icon name={data.platform} />
                    </div>
                    <div className="absolute bottom-0 w-full whitespace-nowrap overflow-hidden overflow-ellipsis p-1 bg-black bg-opacity-80 text-xs text-light-100 z-10">
                        By <span className="font-semibold">{data.credits.join(" & ")}</span>
                    </div>
                    <div className="absolute top-0 left-0 w-full h-full">
                        <GalleryThumbnail thumbnail={data.thumbnail} hasBlur={false} />
                    </div>
                </div>
            </div>
        </a>
    )
}

const SearchResultMobile = ({ data }: { data: SearchResult }) => {
    return (
        <div className="flex lg:hidden flex-col items-center space-y-6">
            <img className="rounded" src={data.parameters.imageUrl} style={{ maxWidth: "75vw", maxHeight: "50vh" }} />
            <div className="text-center">
                <div className="text-muted">
                    Searched {data.stats.count.toLocaleString()} images in {data.stats.elapsedMilliseconds.toLocaleString()} ms
                </div>
                <div className="text-3xl">
                    {
                        {
                            0: "Oh noes! Couldn't find what you're looking for",
                            1: "Jolly good! We found a similar image"
                        }[data.probableResults.length] || "Jolly good! We found similar images"
                    }
                </div>
            </div>
            <div className="flex flex-wrap justify-evenly w-full">
                {data.probableResults.concat(data.improbableResults).slice(0, 12).map(item => (
                    <GalleryCard key={item.id} data={item}></GalleryCard>
                ))}
            </div>
        </div>
    )
};

export default SearchResultMobile
