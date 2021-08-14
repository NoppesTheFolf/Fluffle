import * as React from 'react'

const ImageRating = {
    Safe: 'SAFE',
    Explicit: 'EXPLICIT'
};

const SearchConfig = function () {
    if (typeof localStorage === 'undefined') {
        return {}
    }

    function readIncludeNsfw() {
        if (localStorage.getItem('rating') != null) {
            return localStorage.getItem('rating') === ImageRating.Explicit;
        }

        return false;
    }

    const [includeNsfw, setIncludeNsfw] = React.useState(readIncludeNsfw());

    return {
        includeNsfw,
        setIncludeNsfw(includeNsfw: boolean) {
            localStorage.setItem('rating', includeNsfw ? ImageRating.Explicit : ImageRating.Safe);
            setIncludeNsfw(includeNsfw);
        }
    }
};

export default SearchConfig
