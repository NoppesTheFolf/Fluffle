import * as React from 'react'
import { SearchResult, SearchResultItem } from '../services/api'
import { Gallery, GalleryRow, GalleryRowImage } from '../services/gallery'
import GalleryThumbnail from './gallery-thumbnail'
import Icon from './icon'

const GalleryDesktopCard = ({ image, shouldBlur }: { image: GalleryRowImage<SearchResultItem>, shouldBlur: boolean }) => {
    const [hasHover, setHasHover] = React.useState(false);

    return (
        <a href={image.data.location} rel="noreferrer" target="_blank" onMouseEnter={() => setHasHover(true)} onMouseLeave={() => setHasHover(false)} className="relative rounded overflow-hidden select-none force-light-100" style={{ width: image.width, height: image.height }}>
            <div className={`absolute shadow px-1 py-0.5 flex items-center space-x-1 bg-gradient-${image.data.match.class} rounded-tl rounded-br z-20`}>
                <span className="w-6">
                    <Icon name={image.data.platform} />
                </span>
                <div>{(image.data.score * 100).toFixed(2)}%</div>
            </div>
            <div className={`absolute w-full h-full bg-black transition-colors z-10 ${hasHover ? 'bg-opacity-20' : 'bg-opacity-0'}`}></div>
            <div className={`absolute bottom-0 w-full px-1 py-1.5 whitespace-nowrap overflow-hidden overflow-ellipsis transition-opacity bg-black bg-opacity-80 text-xs z-20 opacity-0 ${hasHover ? 'opacity-100' : ''}`}>
                By <span className="font-semibold">{image.data.credits.join(" & ")}</span>
            </div>
            <GalleryThumbnail thumbnail={image.data.thumbnail} hasBlur={shouldBlur && !hasHover} />
        </a>
    )
}

const GalleryDesktop = ({ data, width, targetHeight, maximumHeight, shouldBlur }: { data: SearchResultItem[], width: number, targetHeight: number, maximumHeight: number, shouldBlur: boolean }) => {
    const gallery = new Gallery<SearchResultItem>(targetHeight, maximumHeight);
    const [render, setRender] = React.useState<GalleryRow<SearchResultItem>[]>()

    React.useEffect(() => {
        renderGallery();
    }, [width]);

    function renderGallery() {
        data.forEach(result => {
            let aspectRatio = result.thumbnail.width / result.thumbnail.height;

            let width = result.thumbnail.width;
            let height = result.thumbnail.height;

            // Images which are either very tall or wide, can screw with the gallery's
            // ability to fit them nicely in the grid. So, in order to fix that, we
            // force those images to be displayed as squares instead, just like on the mobile UI.
            if (aspectRatio < 0.6 || aspectRatio > 2) {
                width = 250;
                height = 250;
            }

            gallery.addImage(width, height, result);
        });

        const newRender = gallery.render(width, 6);

        const minNumberOfImages = 12;
        const minNumberOfRows = 3;
        let currentNumberOfImages = 0;
        let i = 0;
        for (; i < newRender.length; i++) {
            var currentRow = newRender[i];
            currentNumberOfImages += currentRow.images.length;

            if (currentNumberOfImages >= minNumberOfImages && minNumberOfRows <= i + 1) {
                break;
            }
        }

        setRender(newRender.slice(0, i + 1));
    }

    return (
        <div className="flex flex-col space-y-3">
            {render != null && render.map(row => (
                <div key={row.images.map(i => i.data.id).join("-")} className={`flex ${row.couldFit ? "justify-between" : "justify-start space-x-3"}`}>
                    {row.images.map(image => (
                        <GalleryDesktopCard key={image.data.id} image={image} shouldBlur={shouldBlur} />
                    ))}
                </div>
            ))}
        </div>
    )
}

const SearchResultDesktop = ({ data }: { data: SearchResult }) => {
    const [hideImprobable, setHideImprobable] = React.useState(true);
    const [width, setWidth] = React.useState(0);
    const containerRef: React.RefObject<HTMLDivElement> = React.useRef();

    function onResize() {
        setWidth(containerRef.current.clientWidth);
    }

    React.useEffect(() => {
        onResize();
        window.addEventListener('resize', onResize);

        return () => {
            window.removeEventListener('resize', onResize);
        };
    }, []);

    return (
        <div className="hidden lg:flex flex-col space-y-6" ref={containerRef}>
            <div className="flex justify-center items-center space-x-3">
                <img className="rounded" src={data.parameters.imageUrl} style={{ maxWidth: "50%", height: "100%", maxHeight: "300px" }} />
                <div className="max-w-xl">
                    <div className="text-muted">
                        Searched {data.stats.count.toLocaleString()} images in {data.stats.elapsedMilliseconds.toLocaleString()} ms
                    </div>
                    <div className="text-4xl font-light">
                        {
                            {
                                0: "Oh noes! Looks like we couldn't find what you're looking for",
                                1: "Jolly good! We found an image similar to the one you submitted"
                            }[data.probableResults.length] || "Jolly good! We found images similar to the one you submitted"
                        }
                    </div>
                </div>
            </div>
            {width != 0 && data.probableResults.length > 0 &&
                <GalleryDesktop shouldBlur={false} data={data.probableResults} width={width} targetHeight={250} maximumHeight={250} />
            }
            {width != 0 && data.improbableResults.length > 0 &&
                <div className="flex flex-col space-y-2">
                    <div className="flex items-center space-x-1.5">
                        <button className="btn btn-sm btn-secondary space-x-1" style={{ lineHeight: 1 }} onClick={() => setHideImprobable(!hideImprobable)}>
                            <span>
                                <Icon name={hideImprobable ? "visibility" : "visibility_off"} />
                            </span>
                            <span>{hideImprobable ? "Show" : "Hide"} improbable matches</span>
                        </button>
                        <span className="text-sm text-muted hidden lg:inline">We've hidden improbable matches to keep you from viewing content you might experience as disturbing</span>
                    </div>
                    <GalleryDesktop shouldBlur={hideImprobable} data={data.improbableResults} width={width} targetHeight={200} maximumHeight={250} />
                </div>
            }
        </div>
    )
}

export default SearchResultDesktop
