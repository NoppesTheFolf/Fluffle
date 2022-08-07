import * as React from 'react'
import Layout from '../../components/layout'
import Api from '../../services/api'
import GalleryThumbnail from '../../components/gallery-thumbnail'
import Icon from '../../components/icon'
import Loader from '../../components/loader'
import ShortUuidDateTime from '../../services/short-uuid-date-time'
import { DateTime } from 'luxon'

const TelegramAlbumPage = ({ id }) => {
    const messages = {
        PROCESSING: ['Working on it!', 'Fluffle is busy processing this album. Please give it some time.'],
        NOT_FOUND: ['Album not found', 'The referenced Telegram album does not seem to exist.'],
        UNAVAILABLE: ['Yikes!', 'The Telegram album could not be loaded at the moment. This might indicate that Fluffle is partially offline. Please try again later.']
    }
    const [data, setData] = React.useState<any>();
    const [message, setMessage] = React.useState<string[]>();

    React.useEffect(() => {
        Api.mediaGroup(id).subscribe({
            next: result => {
                setData(result.data);
            },
            error: error => {
                if (error.response?.status == 404) {
                    let startedAt = ShortUuidDateTime.fromString(id);

                    let message = DateTime.utc().diff(startedAt, 'minutes').toObject().minutes! < 6
                        ? messages.PROCESSING
                        : messages.NOT_FOUND
                    setMessage(message);

                    return;
                }

                setMessage(messages.UNAVAILABLE);
            }
        });
    }, []);

    return (
        <Layout center={true} title="Album sources">
            {message &&
                <div className="flex justify-center">
                    <div className="text-center prose max-w-lg">
                        <h1 className="m-0">{message[0]}</h1>
                        <p className="text-muted">{message[1]}</p>
                    </div>
                </div>
            }
            {!message && !data &&
                <div className="w-full flex justify-center items-center">
                    <Loader />
                </div>
            }
            {data &&
                <div className="space-y-2">
                    <div className="w-full flex flex-col sm:items-center prose max-w-none">
                        <h1 className="m-0">Album sources</h1>
                    </div>
                    <div className="space-y-2 sm:space-y-0 sm:flex sm:flex-wrap justify-center">
                        {data && data.map((x, xi) => {
                            return (
                                <div key={xi} className="sm:w-1/2 lg:w-1/3 xl:w-1/4 2xl:w-1/5 flex rounded">
                                    <div className="w-full sm:p-2">
                                        <div className="w-full bg-dark-300 rounded p-2">
                                            <div className="w-full  square relative rounded sm:h-full overflow-hidden select-none">
                                                <div className="absolute top-0 left-0 w-full h-full">
                                                    {x.image &&
                                                        <GalleryThumbnail thumbnail={{ location: Api.mediaGroupThumbnailUrl(id, x.image.id + '.jpg'), width: x.image.width, height: x.image.height, centerX: x.image.centerX, centerY: x.image.centerY }} hasBlur={false} />
                                                    }
                                                </div>
                                                <img className="absolute rounded" />
                                                <div className="absolute space-x-1 p-1 bottom-0 left-0 flex justify-end w-full">
                                                    {x.results && x.results.map(result =>
                                                        <a key={result.id} className="relative btn force-light-100" href={result.location} rel="noreferrer" target="_blank">
                                                            <span className="w-6 h-6 z-10">
                                                                <Icon name={result.platform} />
                                                            </span>
                                                            <div className="absolute opacity-90 w-full h-full">
                                                                <div className="w-full h-full rounded bg-gradient-to-br from-dark-300 to-dark-500"></div>
                                                            </div>
                                                        </a>
                                                    )}
                                                    {(!x.results || x.results.length == 0) &&
                                                        <div className="relative btn force-light-100">
                                                            <span className="z-10">
                                                                No sources found
                                                            </span>
                                                            <div className="absolute opacity-90 w-full h-full">
                                                                <div className="w-full h-full rounded bg-gradient-to-br from-dark-300 to-dark-500"></div>
                                                            </div>
                                                        </div>
                                                    }
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
            }
        </Layout>
    )
}

export default TelegramAlbumPage
