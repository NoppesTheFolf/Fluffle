import * as React from 'react'

const ImageRating = {
    Safe: 'SAFE',
    Explicit: 'EXPLICIT'
};

const SearchConfig = function () {
    const ratingKey = 'rating';

    if (typeof localStorage === 'undefined') {
        return {
            includeNsfw: false,
            setIncludeNsfw: () => {}
        };
    }

    function readIncludeNsfw() {
        if (localStorage.getItem(ratingKey) != null) {
            return localStorage.getItem(ratingKey) === ImageRating.Explicit;
        }

        return true;
    }

    const [includeNsfw, setIncludeNsfw] = React.useState(readIncludeNsfw());

    return {
        includeNsfw,
        setIncludeNsfw(includeNsfw: boolean) {
            localStorage.setItem(ratingKey, includeNsfw ? ImageRating.Explicit : ImageRating.Safe);
            setIncludeNsfw(includeNsfw);
        }
    }
};

export default SearchConfig
