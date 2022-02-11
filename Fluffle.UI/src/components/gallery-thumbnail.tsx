import * as React from 'react'
import { galleryThumbnail, blur, skeleton } from './gallery-thumbnail.module.scss'
import { SearchResultThumbnail } from '../services/api'
import classNames from 'classnames';

interface GalleryThumbnailProps {
    thumbnail: SearchResultThumbnail,
    hasBlur: boolean
}

const GalleryThumbnail = ({ thumbnail, hasBlur }: GalleryThumbnailProps) => {
    const [hasBeenLoaded, setHasBeenLoaded] = React.useState(false);
    const [error, setError] = React.useState(null);

    function load(element: HTMLImageElement) {
        if (element == null) return;

        const image = new Image();
        image.onload = () => {
            element.src = image.src;
            setHasBeenLoaded(true);
            setError(null);
        };

        image.onerror = () => {
            setError('Fluffle offline');
        };

        image.src = thumbnail.location;
    }

    return (
        <div className="relative w-full h-full bg-dark-400 text-light-100">
            <img className={hasBlur ? classNames(galleryThumbnail, blur) : galleryThumbnail} style={{
                objectPosition: thumbnail.centerX + "% " + thumbnail.centerY + "%",
                opacity: hasBeenLoaded ? "1" : "0"
            }} ref={element => load(element)} />
            <div className={classNames("absolute w-full h-full top-0 left-0 text-2xl flex text-center justify-center items-center p-3", { hidden: error == null })}>
                {error}
            </div>
            <div className={classNames(skeleton, { hidden: hasBeenLoaded || error != null })}></div>
        </div>
    )
}

export default GalleryThumbnail
