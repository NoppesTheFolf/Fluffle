import { HttpEventType } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiService, SearchResult, SearchResultImage, SearchResultImageMatch } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class SearchResultService {
  SearchState = SearchState;

  result: SearchResult;
  excellentImages: SearchResultImage[];
  otherImages: SearchResultImage[];
  state: SearchState = SearchState.Idle;
  errorMessage: string;
  progress: number = 0;
  hideImprobable: boolean = true;

  constructor(private api: ApiService) { }

  search(croppedImage: Blob, image: Blob) {
    this.result = null;
    this.excellentImages = null;
    this.otherImages = null;
    this.state = SearchState.Uploading;
    this.errorMessage = null;
    this.progress = 0;
    this.hideImprobable = true;

    this.api.search(croppedImage, image).subscribe(event => {
      if (event.http.type === HttpEventType.UploadProgress) {
        this.progress = Math.round((event.http.loaded / event.http.total * 100));
      }

      if (event.http.type === HttpEventType.Response) {
        if (event.http.ok) {
          this.result = event.result;
          this.excellentImages = this.result.images.filter(i => i.match === SearchResultImageMatch.Excellent);
          this.otherImages = this.result.images.filter(i => i.match !== SearchResultImageMatch.Excellent);
          this.state = SearchState.Finished;
          return;
        }
      }
    }, (response: any) => {
      this.state = SearchState.Error;
      this.errorMessage = "Something went horribly wrong and we're not quite sure what.";

      if (response.name === "TimeoutError") {
        this.errorMessage = "Fluffle seems to be partially offline, please try again later."
      }

      if (response.error as V1ApiError) {
        let code = response.error.code;
        if (code === "UNSUPPORTED_FILE_TYPE") {
          this.errorMessage = "The file you submitted was of an unsupported file type.";
        } else if (code === "CORRUPT_IMAGE") {
          this.errorMessage = "The image you submitted seems to be corrupt."
        }
      }
    });
  }

  setError(errorMessage: string) {
    this.errorMessage = errorMessage;
    this.state = SearchState.Error;
  }
}

export class V1ApiError {
  code: string;
  message: string;
}

export enum SearchState {
  Idle,
  DownScaling,
  Uploading,
  Finished,
  Error
}
