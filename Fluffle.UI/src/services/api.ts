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

    function searchResultUrl(id, extension) {
        return urlcat(process.env.GATSBY_SEARCH_RESULT_URL as string, ':fileName', { fileName: `${id}.${extension}` });
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

            return axios.post(url('/exact-search'), formData, config)
                .then(response => {
                    const data = response.data;

                    const results = data.results.map(x => {
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

                    return {
                        parameters: {
                            imageUrl: URL.createObjectURL(file),
                            includeNsfw: includeNsfw,
                            fromQuery: false
                        },
                        id: data.id,
                        probableResults: results.filter(r => r.match === Match.Excellent),
                        improbableResults: results.filter(r => r.match !== Match.Excellent)
                    } as SearchResult
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
        searchResultUrl,
        searchResult(id: string, maxAttempts: number, delayDue: number, retryStatusCodes: number[]) {
            // TODO
            // return b2Retrieve(searchResultUrl(id, 'json'), undefined, maxAttempts, delayDue, retryStatusCodes).pipe(
            //     map(response => {
            //         return processSearchData(response.data, searchResultUrl(id, 'jpg'), undefined, true);
            //     })
            // );
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
