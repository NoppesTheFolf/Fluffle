import axios, { AxiosRequestConfig } from 'axios';
import { defer } from 'rxjs';
import { retryWhen, delay, tap, map } from 'rxjs/operators';
import urlcat, { ParamMap } from 'urlcat';

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
    credits: string;
}

export interface SearchResult {
    parameters: {
        imageUrl: string,
        includeNsfw: boolean
        fromQuery: boolean
    },
    id: string,
    stats: {
        count: number,
        elapsedMilliseconds: number
    },
    probableResults: SearchResultItem[],
    improbableResults: SearchResultItem[]
}

const Api = function () {
    const sizeLimit = 4194304;

    function url(path: string, params: ParamMap = {}) {
        const baseUrl = urlcat(process.env.GATSBY_API_URL as string, 'v1');
        return urlcat(baseUrl, path, params);
    }

    function mediaGroupIndexUrl(id, file) {
        return mediaGroupUrl(process.env.GATSBY_TELEGRAM_MEDIA_GROUP_INDEX_URL, id, file);
    }

    function mediaGroupThumbnailUrl(id, file) {
        return mediaGroupUrl(process.env.GATSBY_TELEGRAM_MEDIA_GROUP_THUMBNAIL_URL, id, file);
    }

    function mediaGroupUrl(baseUrl, id, file) {
        return urlcat(baseUrl, ':id/:file', { id: id, file: file });
    }

    function searchResultUrl(id, extension) {
        return urlcat(process.env.GATSBY_SEARCH_RESULT_URL as string, ':fileName', { fileName: `${id}.${extension}` });
    }

    function processSearchData(data, imageUrl, includeNsfw: boolean | undefined = undefined, fromQuery = false) {
        const results = data.results.map(r => {
            r.credits = r.credits.map(c => c.name).join(" & ");
            r.score = (r.score - 0.5) * 2;
            r.score = Math.sign(r.score) === -1 ? 0 : r.score;
            r.match = r.match === 'exact' ? Match.Excellent : r.match === 'unlikely' ? Match.Unlikely : Match.Doubtful;
            return r;
        });

        return {
            parameters: {
                imageUrl: imageUrl,
                includeNsfw: includeNsfw,
                fromQuery: fromQuery
            },
            id: data.id,
            stats: data.stats,
            probableResults: results.filter(r => r.match === Match.Excellent),
            improbableResults: results.filter(r => r.match !== Match.Excellent)
        } as SearchResult
    }

    function b2Retrieve(url, timeout = 4000, maxAttempts = 3, delayDue = 500, retryStatusCodes: number[] = []) {
        // Backblaze B2 is laughably unreliable. We will attempt to retrieve the index three
        // times and retry when the request times out or errors with a 500 or 503 response. It would be neat
        // if we could implement this same logic for loading the gallery.
        retryStatusCodes = [500, 503].concat(retryStatusCodes);
        return defer(() => {
            return axios.get(url, { timeout: timeout });
        }).pipe(
            retryWhen(errors => {
                let errorAttempt = 1;

                return errors.pipe(
                    tap(error => {
                        if (errorAttempt > maxAttempts) {
                            throw error;
                        }

                        if (error.code === 'ECONNABORTED' || retryStatusCodes.includes(error.response?.status)) {
                            errorAttempt++;
                            return;
                        }

                        throw error;
                    }),
                    delay(delayDue)
                );
            })
        );
    }

    return {
        mediaGroupThumbnailUrl,
        search(file: Blob, includeNsfw: boolean, limit: number = 32, createLink: boolean = false, config: AxiosRequestConfig | undefined = undefined) {
            if (file.size > sizeLimit) {
                return Promise.reject('The selected file is over the 4 MiB limit.');
            }

            const formData = new FormData();
            formData.append('file', file);
            formData.append('limit', String(limit));
            formData.append('includeNsfw', String(includeNsfw));
            formData.append('createLink', String(createLink));

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
                    return processSearchData(response.data, URL.createObjectURL(file), includeNsfw, false);
                })
        },
        processSearchData,
        searchResultUrl,
        searchResult(id: string, maxAttempts: number, delayDue: number, retryStatusCodes: number[]) {
            return b2Retrieve(searchResultUrl(id, 'json'), undefined, maxAttempts, delayDue, retryStatusCodes).pipe(
                map(response => {
                    return processSearchData(response.data, searchResultUrl(id, 'jpg'), undefined, true);
                })
            );
        },
        async status(config: AxiosRequestConfig | undefined = undefined) {
            const response = await axios.get(url('status'), config);

            return response.data.map(status => {
                status.scrapedPercentage = status.isComplete ? 100 : Math.round(status.storedCount / status.estimatedCount * 100);
                status.indexedPercentage = Math.round(status.indexedCount / (status.isComplete ? status.storedCount : status.estimatedCount) * 100);

                return status;
            });
        },
        mediaGroup(id: string) {
            return b2Retrieve(mediaGroupIndexUrl(id, 'index.json'));
        }
    }
}();

export default Api
