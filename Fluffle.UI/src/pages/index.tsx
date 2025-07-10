import * as React from 'react'
import Layout from '../components/layout'
import Banner from '../components/banner'
import { Link, navigate } from 'gatsby'
import ProgressBar from '../components/progress-bar'
import ProgressBarPart from '../components/progress-bar-part'
import Api, { SearchResult } from '../services/api'
import SearchResultDesktop from '../components/search-result-desktop'
import Icon from '../components/icon'
import SearchConfig from '../services/search-config'
import SearchResultMobile from '../components/search-result-mobile'
import classNames from 'classnames'
import { dropZone, dropZoneActive } from './index.module.scss'

import SEO from '../components/seo'
export const Head = () => (
    <SEO title="Reverse search" description="Reverse image search furry artwork and find the source on Fur Affinity, Twitter, e621 and more!" />
)

const State = {
    ERROR: -1,
    IDLE: 0,
    WAITING_FOR_BROWSER_EXTENSION: 1,
    PREPROCESSING: 2,
    UPLOADING: 3,
    PROCESSING: 4,
    DONE: 5
};

const SearchPage = ({ forBrowserExtension, searchResult }) => {
    let searchConfig = SearchConfig();

    const containerObserverTimeout = 500;
    let containerObserver: ResizeObserver | undefined;
    const [reverseSearchTime, setReverseSearchTime] = React.useState(0);

    const canvasRef: React.RefObject<HTMLCanvasElement | null | undefined> = React.useRef();
    const dataUrlRef: React.RefObject<HTMLInputElement | null | undefined> = React.useRef();

    const [state, setState] = React.useState(forBrowserExtension ? State.WAITING_FOR_BROWSER_EXTENSION : State.IDLE);
    const [data, setData] = React.useState<SearchResult | undefined>(searchResult);
    const [errorMessage, setErrorMessage] = React.useState<string | undefined>();
    const [progress, setProgress] = React.useState(0);
    const [hasDrag, setHasDrag] = React.useState(false);

    React.useEffect(() => {
        if (searchResult != null) {
            setReverseSearchTime(new Date().getTime());
            setState(State.DONE);
        }
    }, [searchResult]);

    function setError(message) {
        setErrorMessage(message);
        setState(State.ERROR);
    }

    function calculateThumbnailSize(width, height, target) {
        let determineSize = (sizeOne, sizeTwo, sizeOneTarget) => {
            var aspectRatio = sizeOneTarget / sizeOne;

            return Math.round(aspectRatio * sizeTwo);
        };

        if (width === height) {
            return [target, target];
        }

        return width > height
            ? [determineSize(height, width, target), target]
            : [target, determineSize(width, height, target)];
    }

    function search(value: FileList | Blob) {
        // Ignore call if no files are provided
        if (value instanceof FileList && value.length === 0) {
            return;
        }

        setState(State.IDLE);
        setData(undefined);
        setErrorMessage(undefined);
        setProgress(0);

        let file: Blob;
        if (value instanceof FileList) {
            if (value.length > 1) {
                setError('You can only reverse search one image at a time.');
                return;
            }
            file = value[0];
        } else {
            file = value;
        }

        setState(State.PREPROCESSING);

        const image = new Image();
        const canvas = canvasRef.current!;
        image.onload = () => {
            const target = 512;
            const thumbnailSize = calculateThumbnailSize(image.width, image.height, target);

            // In the first place we scaled down the image to a fixed size (250x250), but that
            // caused such a significant loss in image quality in some instances that we had to
            // scale preserving the aspect ratio of the original image  
            canvas.width = thumbnailSize[0];
            canvas.height = thumbnailSize[1];
            const ctx = canvas.getContext('2d')!;

            // Add white background
            ctx.beginPath();
            ctx.rect(0, 0, canvas.width, canvas.height);
            ctx.fillStyle = 'white';
            ctx.fill();

            // Draw to be reverse searched image
            ctx.drawImage(image, 0, 0, canvas.width, canvas.height);

            // Convert image drawn on canvas to blob
            const dataUri = canvas.toDataURL('image/jpeg');
            fetch(dataUri).then(response => response.blob()).then(thumbnail => {
                searchInternal(thumbnail);
            });
        };

        // The error might simply be that the image format isn't supported by the canvas.
        // Therefore, we should still send it to the server.
        image.onerror = () => {
            searchInternal(file);
        }

        image.src = URL.createObjectURL(file);
    }

    function searchInternal(file: Blob) {
        setState(State.UPLOADING);
        Api.search(file, searchConfig.includeNsfw, undefined, false, {
            onUploadProgress: e => {
                const progress = Math.round(e.loaded / e.total * 100);

                setProgress(progress);

                if (progress === 100) {
                    setState(State.PROCESSING);
                }
            }
        }).then(data => {
            setReverseSearchTime(new Date().getTime());
            setData(data);
            setState(State.DONE);
        }).catch(message => {
            setError(message);
        });
    }

    function onContainerChanged(ref: HTMLElement) {
        if (ref == null) {
            containerObserver?.disconnect();
            return;
        }

        // Allow the time to be updated from within the observer its callback by making a copy of it.
        let containerObserverTime = reverseSearchTime;
        containerObserver = new ResizeObserver(() => {
            const now = new Date().getTime();
            if (now - containerObserverTime > containerObserverTimeout) {
                return;
            }

            containerObserverTime = now;
            ref.scrollIntoView({
                behavior: 'smooth'
            });
        });
        containerObserver.observe(ref);
    }

    function onSelect(event) {
        search(event.target.files);
    }

    function onProgrammaticSubmit(event) {
        const dataUrl = dataUrlRef.current!.value;
        if (dataUrl == null || dataUrl === '') {
            navigate('/');
            return;
        }

        fetch(dataUrl).then(request => request.blob()).then(blob => {
            search(blob);
        });
    }

    function onDragover(event) {
        setHasDrag(true);
        event.preventDefault();
    }

    function onDragLeave(event) {
        setHasDrag(false);
        event.preventDefault();
    }

    function onDrop(event) {
        // Prevent file from being opened
        event.preventDefault();
        setHasDrag(false);

        if (event.dataTransfer.files.length == 0) {
            setError('Did you drop a file which originates from the browser? Due to your browser its limitations, Fluffle cannot access those files. Save the file to your device first, then submit this file instead.')
            return;
        }

        search(event.dataTransfer.files);
    }

    // Safari doesn't handle clickable elements the same way as other browsers.
    // We need to explicitly cancel the opening of a file dialog when another element
    // is clicked on. We use a custom ignore attribute for that.
    function onLabelClick(event) {
        if (event.target.hasAttribute('data-ignore')) {
            event.preventDefault();
        }
    }

    return (
        <Layout center={true} requireBrowser={true} maxWidth="7xl">
            <div className="w-full flex flex-col items-center space-y-12">
                <div className="flex flex-col justify-center items-center">
                    <div className="mb-4 flex justify-center w-auto">
                        <Link className="flex justify-center sm:block" to="/">
                            <Banner />
                        </Link>
                    </div>
                    <div className="text-muted text-center italic">
                        A reverse image search service for the furry community
                    </div>
                </div>
                <input className="hidden" type="file" id="image" onChange={onSelect} />
                <input className="hidden" type="text" id="image-data-url" ref={dataUrlRef} />
                <button className="hidden" id="programmatic-submit" onClick={onProgrammaticSubmit}></button>
                <canvas className="hidden" ref={canvasRef}></canvas>
                {![State.PREPROCESSING, State.UPLOADING, State.PROCESSING].includes(state) &&
                    <div className={classNames("flex w-full max-w-4xl flex-col space-y-3", { "hidden": state === State.WAITING_FOR_BROWSER_EXTENSION })}>
                        {state === State.ERROR &&
                            <div className="flex items-center bg-gradient-danger space-x-3 p-4 rounded">
                                <Icon name="report-problem" />
                                <span>{errorMessage}</span>
                            </div>
                        }
                        <label htmlFor="image" onClick={e => onLabelClick(e)} onDragOver={e => onDragover(e)} onDrop={e => onDrop(e)} onDragLeave={e => onDragLeave(e)}>
                            <div className={hasDrag ? classNames(dropZone, dropZoneActive) : dropZone}>
                                <div className="text-5xl hidden sm:block">Drag 'n drop a fluffy critter here</div>
                                <div className="hidden sm:block">Or</div>
                                <div className="flex flex-col space-y-2">
                                    <div className="flex flex-col sm:flex-row items-center space-x-2 space-y-2 sm:space-y-0">
                                        <label htmlFor="image" className="btn btn-primary text-xl sm:text-base">
                                            <span className="mr-2">
                                                <Icon name="photo-size-select-actual" />
                                            </span>
                                            <span className="hidden sm:inline">Select a floof</span>
                                            <span className="inline sm:hidden">Select a fluffy critter</span>
                                        </label>
                                        <div className="flex btn-group">
                                            <button onClick={() => searchConfig.setIncludeNsfw(false)} className={`btn btn-${searchConfig.includeNsfw ? "secondary" : "info"}`} data-ignore>SFW</button>
                                            <button onClick={() => searchConfig.setIncludeNsfw(true)} className={`btn btn-${searchConfig.includeNsfw ? "danger" : "secondary"}`} data-ignore>NSFW</button>
                                        </div>
                                    </div>
                                    <div className="text-sm text-muted">{!searchConfig.includeNsfw ? "Twitter isn't included in SFW mode." : 'Search both SFW and NSFW images.'}</div>
                                </div>
                                <div className="text-muted">Fluffle also has a Telegram bot and a browser extension, interested? Check out the <Link to="/tools/">tools page</Link>.</div>
                            </div>
                        </label>
                    </div>
                }

                {[State.WAITING_FOR_BROWSER_EXTENSION, State.PREPROCESSING, State.UPLOADING, State.PROCESSING].includes(state) &&
                    <div className="w-full max-w-xl flex flex-col space-y-3 items-center">
                        <ProgressBar>
                            <ProgressBarPart color="bg-primary" isStriped={true} isAnimated={true} percentage={progress}></ProgressBarPart>
                        </ProgressBar>
                        <span className="text-center">{state === State.WAITING_FOR_BROWSER_EXTENSION ? 'Waiting for the browser extension to interact with Fluffle' : state === State.PREPROCESSING ? "Preprocessing" : state === State.UPLOADING ? "Uploading" : "Processing"}...</span>
                    </div>
                }

                {state === State.DONE &&
                    <div className="w-full bg-dark-300 p-2 rounded" ref={onContainerChanged}>
                        <SearchResultDesktop data={data} />
                        <SearchResultMobile data={data} />
                    </div>
                }
            </div>
        </Layout>
    )
}

SearchPage.defaultProps = {
    forBrowserExtension: false,
    searchResult: undefined
}

export default SearchPage
