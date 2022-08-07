import * as React from 'react'
import { Match, SearchResult, SearchResultItem } from '../services/api';
import GalleryThumbnail from './gallery-thumbnail';
import Icon from './icon';
import CreateLinkButton from '../components/create-link-button'

const GalleryCard = ({ data, enableBlur, onClick, seq }: { data: SearchResultItem, enableBlur: boolean, onClick: Function, seq: number }) => {
    const [hasBeenClicked, setHasBeenClicked] = React.useState(false);

    React.useEffect(() => {
        setHasBeenClicked(false);
    }, [seq]);

    const [hasBlur, setHasBlur] = React.useState(false);
    React.useEffect(() => {
        setHasBlur(enableBlur && !hasBeenClicked);
    }, [hasBeenClicked, enableBlur]);

    function handleClick(e: React.MouseEvent<HTMLAnchorElement>) {
        if (!hasBlur) {
            return;
        }

        setHasBeenClicked(true);
        e.preventDefault();
        onClick();
    }

    return (
        <a className="square w-1/2 sm:w-1/3 md:w-1/4 p-1 block relative force-light-100" onClick={handleClick} href={data.location} target="_blank" rel="noreferrer">
            <div className="absolute left-0 top-0 w-full h-full p-inherit">
                <div className="relative w-full h-full rounded overflow-hidden">
                    <div className={`absolute top-0 left-0 w-7 p-0.5 bg-gradient-${data.match.class} rounded-tl rounded-br z-10`}>
                        <Icon name={data.platform} />
                    </div>
                    <div className={`absolute transition-opacity bottom-0 w-full whitespace-nowrap overflow-hidden overflow-ellipsis p-1 bg-black bg-opacity-80 text-xs z-10 opacity-0 ${hasBlur ? '' : 'opacity-100'}`}>
                        {data.credits && <span>By <span className="font-semibold">{data.credits}</span></span>}
                        {!data.credits && <span className="font-semibold">Unknown artist</span>}
                    </div>
                    <div className="absolute top-0 left-0 w-full h-full">
                        <GalleryThumbnail thumbnail={data.thumbnail} hasBlur={hasBlur} />
                    </div>
                </div>
            </div>
        </a>
    )
}

const GalleryMobile = ({ results, hideImprobable, onClick, seq }: { results: SearchResultItem[], hideImprobable: boolean, onClick: Function, seq: number }) => {
    return (
        <div className="flex flex-wrap w-full">
            {results.slice(0, 12).map(item => (
                <GalleryCard onClick={onClick} seq={seq} key={item.id} data={item} enableBlur={item.match !== Match.Excellent && hideImprobable}></GalleryCard>
            ))}
        </div>
    )
}

const SearchResultMobile = ({ data }: { data: SearchResult }) => {
    const [hideImprobable, setHideImprobable] = React.useState(true);
    const [shouldHide, setShouldHide] = React.useState(false);
    const [seq, setSeq] = React.useState(0);

    function onClick() {
        setShouldHide(true);
    }

    function toggleHide() {
        setHideImprobable(shouldHide);
        setShouldHide(!shouldHide);
        setSeq(seq + 1);
    }

    return (
        <div className="lg:hidden space-y-6">
            <div className="flex flex-col items-center space-y-6">
                <img className="rounded" src={data.parameters.imageUrl} style={{ maxWidth: "75vw", maxHeight: "50vh" }} />
                <div className="text-center">
                    <div className="text-muted">
                        <span>Searched {data.stats.count.toLocaleString()} images</span>
                        {!data.parameters.fromQuery &&
                            <span> in {data.stats.elapsedMilliseconds.toLocaleString()} ms</span>
                        }
                    </div>
                    <div className="text-3xl">
                        {
                            {
                                0: "Couldn't find what you're looking for",
                                1: "We found a similar image"
                            }[data.probableResults.length] || "We found similar images"
                        }
                    </div>
                </div>
            </div>
            {!data.parameters.fromQuery &&
                <CreateLinkButton data={data} />
            }
            <div className="space-y-3">
                <div className="space-y-1">
                    <div className="px-1 space-y-1">
                        <div className="text-xs text-center text-muted">
                            We've hidden improbable matches to keep you from viewing content you might experience as unpleasant
                        </div>
                        <button className="btn btn-sm btn-secondary w-full space-x-1" onClick={toggleHide}>
                            <span>
                                <Icon name={shouldHide ? "visibility-off" : "visibility"} />
                            </span>
                            <span>{shouldHide ? "Hide" : "Show"} improbable matches</span>
                        </button>
                    </div>
                    <GalleryMobile onClick={onClick} seq={seq} results={data.probableResults.concat(data.improbableResults)} hideImprobable={hideImprobable} />
                </div>
            </div>
        </div>
    )
};

export default SearchResultMobile
