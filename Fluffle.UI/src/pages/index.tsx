import * as React from 'react'
import Layout from '../components/layout'
import Banner from '../components/banner'
import { Link } from 'gatsby'
import ProgressBar from '../components/progress-bar'
import ProgressBarPart from '../components/progress-bar-part'
import Api, { SearchResult } from '../services/api'
import SearchResultDesktop from '../components/search-result-desktop'
import { Helmet } from 'react-helmet'
import Icon from '../components/icon'
import SearchConfig from '../services/search-config'
import SearchResultMobile from '../components/search-result-mobile'
import classNames from 'classnames'
import { dropZone, dropZoneActive } from './index.module.scss'

const State = {
    ERROR: -1,
    IDLE: 0,
    PREPROCESSING: 1,
    UPLOADING: 2,
    PROCESSING: 3,
    DONE: 4
};

const SearchPage = () => {
    let searchConfig = SearchConfig();

    const canvasRef: React.RefObject<HTMLCanvasElement> = React.useRef();
    const containerRef: React.RefObject<HTMLDivElement> = React.useRef();

    const [state, setState] = React.useState(State.IDLE);
    const [data, setData] = React.useState<SearchResult>(null);
    const [errorMessage, setErrorMessage] = React.useState(null);
    const [progress, setProgress] = React.useState(0);
    const [hasDrag, setHasDrag] = React.useState(false);

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

    function search(files: FileList) {
        setState(State.IDLE);
        setData(null);
        setErrorMessage(null);
        setProgress(0);

        if (files.length > 1) {
            setError('You can only reverse search one image at a time.');
            return;
        }
        const file = files[0];

        setState(State.PREPROCESSING);

        const image = new Image();
        const canvas = canvasRef.current;
        image.onload = () => {
            const target = 256;
            const thumbnailSize = calculateThumbnailSize(image.width, image.height, target);

            // In the first place we scaled down the image to a fixed size (250x250), but that
            // caused such a significant loss in image quality in some instances that we had to
            // scale preserving the aspect ratio of the original image  
            canvas.width = thumbnailSize[0];
            canvas.height = thumbnailSize[1];
            const ctx = canvas.getContext('2d');
            ctx.drawImage(image, 0, 0, canvas.width, canvas.height);

            // Convert image drawn on canvas to blob
            const dataUri = canvas.toDataURL('image/png');
            const base64EncodedData = dataUri.split(',')[1];
            const data = atob(base64EncodedData);
            const array = new Uint8Array(data.length);
            for (let i = 0; i < data.length; i++) {
                array[i] = data.charCodeAt(i);
            }
            const thumbnail = new Blob([array]);

            searchInternal(file, thumbnail);
        };

        // The error might simply be that the image format isn't supported by the canvas.
        // Therefore, we should still send it to the server.
        image.onerror = () => {
            searchInternal(file, file);
        }

        image.src = URL.createObjectURL(file);
    }

    function searchInternal(file: Blob, thumbnail: Blob) {
        setState(State.UPLOADING);
        Api.search(file, thumbnail, searchConfig.includeNsfw, 64, {
            onUploadProgress: e => {
                const progress = Math.round(e.loaded / e.total * 100);

                setProgress(progress);

                if (progress === 100) {
                    setState(State.PROCESSING);
                }
            }
        }).then(data => {
            setData(data);
            setState(State.DONE);

            // TODO: Using a timeout has proven unrealiable in the Angular version of the application
            setTimeout(() => {
                containerRef.current.scrollIntoView({
                    behavior: 'smooth'
                });
            }, 250);
        }).catch(message => {
            setError(message);
        });
    }

    function onSelect(event) {
        search(event.target.files);
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
        <Layout center={true} requireBrowser={true} title="Reverse search">
            <Helmet>
                <meta name="description" content="A reverse image search service tailored to the furry community."></meta>
            </Helmet>
            <div className="w-full flex flex-col items-center space-y-12">
                <div className="flex flex-col justify-center items-center">
                    <div className="mb-4 flex justify-center w-auto">
                        <Link to="/">
                            <Banner></Banner>
                            <span className="absolute text-muted uppercase hidden sm:inline">
                                {process.env.VERSION}
                            </span>
                        </Link>
                    </div>
                    <div className="text-muted text-center italic">
                        A reverse image search service tailored to the furry community
                    </div>
                </div>
                <input className="hidden" type="file" id="image" onChange={onSelect} />
                <canvas className="hidden" ref={canvasRef}></canvas>
                {![State.PREPROCESSING, State.UPLOADING, State.PROCESSING].includes(state) &&
                    <div className="flex w-full max-w-4xl flex-col space-y-3">
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
                                <div className="text-muted">Concerned about what happens to your data? Take a look at the <Link to="/about/">about page</Link>.</div>
                            </div>
                        </label>
                    </div>
                }

                {[State.PREPROCESSING, State.UPLOADING, State.PROCESSING].includes(state) &&
                    <div className="w-full max-w-xl flex flex-col space-y-3 items-center">
                        <ProgressBar>
                            <ProgressBarPart color="bg-primary" isStriped={true} isAnimated={true} percentage={progress}></ProgressBarPart>
                        </ProgressBar>
                        <span>{state === State.PREPROCESSING ? "Preprocessing" : state === State.UPLOADING ? "Uploading" : "Processing"}...</span>
                    </div>
                }

                {state === State.DONE &&
                    <div className="w-full bg-dark-300 p-2 rounded" ref={containerRef}>
                        <SearchResultDesktop data={data} />
                        <SearchResultMobile data={data} />
                    </div>
                }
            </div>
        </Layout>
    )
}

export default SearchPage
