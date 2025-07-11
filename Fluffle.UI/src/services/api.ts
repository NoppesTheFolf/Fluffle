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
    probableResults: SearchResultItem[],
    improbableResults: SearchResultItem[]
}

const Api = function () {
    const sizeLimit = 4194304;

    function url(path: string, params: ParamMap = {}) {
        const baseUrl = urlcat(process.env.GATSBY_API_URL as string);
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

    function processExactSearchResponse(data, includeNsfw, imageUrl, fromQuery) {
        let results = data.results.map(x => {
            return {
                id: x.id,
                match: x.match === 'exact' ? Match.Excellent : x.match === 'probable' ? Match.Doubtful : Match.Unlikely,
                platform: x.platform,
                location: x.url,
                isSfw: x.isSfw,
                thumbnail: {
                    width: x.thumbnail.width,
                    centerX: x.thumbnail.centerX,
                    height: x.thumbnail.height,
                    centerY: x.thumbnail.centerY,
                    location: x.thumbnail.url
                },
                credits: x.authors.map(y => y.name).join(' & ')
            } as SearchResultItem
        });

        if (!includeNsfw) {
            results = results.filter(x => x.isSfw);
        }

        return {
            parameters: {
                imageUrl: imageUrl,
                includeNsfw: includeNsfw,
                fromQuery: fromQuery
            },
            id: data.id,
            probableResults: results.filter(r => r.match === Match.Excellent),
            improbableResults: results.filter(r => r.match !== Match.Excellent)
        } as SearchResult
    }

    return {
        mediaGroupThumbnailUrl,
        createLink(file: Blob) {
            const formData = new FormData();
            formData.append('file', file);

            return axios.post(url('create-link'), formData)
                .then(response => {
                    const data = response.data;

                    return data;
                });
        },
        searchById(id: string, includeNsfw: boolean, ) {
            return axios.get(url('/exact-search-by-id', { id: id, limit: 32 }))
                .then(response => {
                    const data = response.data;

                    const p1 = id.substring(0, 2);
                    const p2 = id.substring(2, 4);

                    const imageUrl = urlcat(process.env.GATSBY_CONTENT_API_URL as string, 'users/:p1/:p2/:fileName', { p1: p1, p2: p2, fileName: `${id}.jpg` });

                    return processExactSearchResponse(data, includeNsfw, imageUrl, true);
                })
        },
        searchByFile(file: Blob, includeNsfw: boolean, config: AxiosRequestConfig | undefined = undefined) {
            if (file.size > sizeLimit) {
                return Promise.reject('The selected file is over the 4 MiB limit.');
            }

            const formData = new FormData();
            formData.append('file', file);
            formData.append('limit', String(32));

            return axios.post(url('/exact-search-by-file'), formData, config)
                .then(response => {
                    const data = response.data;

                    return processExactSearchResponse(data, includeNsfw, URL.createObjectURL(file), false);
                })
                .catch(error => {
                    let message = 'Your request caused an error. If you think this is a bug, consider reporting it (see contact) so that it can be fixed.';

                    if (error.code === 'ERR_NETWORK') {
                        message = 'Fluffle seems to be offline, please try again later.';
                    } else {
                        const errorCode = error.response.data?.errors[0].code;
                        if (errorCode == null) {
                            return;
                        }

                        switch (errorCode) {
                            case 'unsupportedFileType':
                                message = 'The submitted file is of an unsupported type.';
                                break;
                            case 'areaTooLarge':
                                message = 'The submitted image its area exceeds the limit of 16MP.';
                                break;
                            case 'corruptFile':
                                message = 'The image you submitted seems to be corrupt.';
                                break;
                        }
                    }

                    return Promise.reject(message);
                });
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
