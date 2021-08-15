import axios, { AxiosRequestConfig } from 'axios';

export const Match = {
    Excellent: {
        class: 'success'
    },
    Doubtful: {
        class: 'warning'
    },
    Unlikely: {
        class: 'danger'
    }
};

export interface SearchResultThumbnail {
    width: number,
    centerX: number,
    height: number,
    centerY: number,
    location: string
}

export interface SearchResultItem {
    id: number;
    score: number;
    match: { class: string };
    platform: string;
    location: string;
    isSfw: boolean;
    thumbnail: SearchResultThumbnail;
    credits: string[];
}

export interface SearchResult {
    parameters: {
        imageUrl: string,
        includeNsfw: boolean
    },
    stats: {
        count: number,
        elapsedMilliseconds: number
    },
    probableResults: SearchResultItem[],
    improbableResults: SearchResultItem[]
}

const Api = function () {
    const sizeLimit = 4194304;

    function url(segment) {
        return `${process.env.API_URL}/v1/${segment}`;
    }

    return {
        search(file: Blob, thumbnail: Blob, includeNsfw: boolean, limit: number, config: AxiosRequestConfig = undefined) {
            if (thumbnail.size > sizeLimit) {
                return Promise.reject('The selected file is over the 4 MiB limit.');
            }

            const formData = new FormData();
            formData.append('file', thumbnail);
            formData.append('limit', String(limit));
            formData.append('includeNsfw', String(includeNsfw));

            return axios.post(url('search'), formData, config)
                .catch(error => {
                    let message = 'Something went horribly wrong and we\'re not quite sure what.';

                    if (error.response) {
                        // The request was made and the server responded with a status code that falls out of the range of 2xx
                        if (error.response.data.errors) return;

                        switch (error.response.data.code) {
                            case 'FILE_TOO_LARGE':
                                message = 'The submitted file is too large to process.';
                                break;
                            case 'UNSUPPORTED_FILE_TYPE':
                                message = 'The submitted file is of an unsupported type.';
                                break;
                            case 'CORRUPT_FILE':
                                message = 'The image you submitted seems to be corrupt.';
                                break;
                            case 'AREA_TOO_LARGE':
                                message = 'The submitted image its area exceeds the limit of 16MP.';
                                break;
                            case 'UNAVAILABLE':
                                message = 'Fluffle is still starting up. Please try again in a bit.';
                                break;
                            case 'KABOOM':
                                message = `You crashed Fluffle, congratulations! Please consider reporting this (see contact). Make sure you can provide the following code if you choose to do so: ${error.response.data.traceId}`;
                                break;
                        }
                    } else if (error.request) {
                        // The request was made but no response was received
                        message = 'Fluffle seems to be partially offline, please try again later.';
                    }

                    return Promise.reject(message);
                }).then<SearchResult>(response => {
                    const data = response.data.results.map(r => {
                        r.match = r.score >= 0.92 ? Match.Excellent : r.score >= 0.85 ? Match.Doubtful : Match.Unlikely;
                        return r;
                    });

                    return {
                        parameters: {
                            imageUrl: URL.createObjectURL(file),
                            includeNsfw: includeNsfw
                        },
                        stats: response.data.stats,
                        probableResults: data.filter(r => r.match === Match.Excellent),
                        improbableResults: data.filter(r => r.match !== Match.Excellent)
                    }
                })
        },
        async status(config: AxiosRequestConfig = undefined) {
            const response = await axios.get(url('status'), config);

            return response.data.map(status => {
                status.scrapedPercentage = status.isComplete ? 100 : Math.round(status.storedCount / status.estimatedCount * 100);
                status.indexedPercentage = Math.round(status.indexedCount / (status.isComplete ? status.storedCount : status.estimatedCount) * 100);

                return status;
            });
        }
    }
}();

export default Api
