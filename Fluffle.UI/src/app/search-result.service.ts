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

  search(file: Blob, image: Blob) {
    if (file.size > 4_194_304) {
      this.state = SearchState.Error;
      this.errorMessage = "The selected file is over the 4 MiB limit.";
      return;
    }

    this.result = null;
    this.excellentImages = null;
    this.otherImages = null;
    this.state = SearchState.Uploading;
    this.errorMessage = null;
    this.progress = 0;
    this.hideImprobable = true;

    this.api.search(file, image).subscribe(event => {
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
        this.errorMessage = "Fluffle seems to be partially offline, please try again later.";
        return;
      }

      switch (response.status) {
        case 403:
          this.errorMessage = "The submitted file is too large to process.";
          break;
        case 415:
          this.errorMessage = "The file you submitted was of an unsupported file type.";
          break;
        case 422:
          this.errorMessage = "The image you submitted seems to be corrupt.";
          break;
        case 503:
          this.errorMessage = "Fluffle is still starting up. Please try again in a bit."
          break;
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
  Preparing,
  Uploading,
  Finished,
  Error
}
