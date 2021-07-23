import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { timeout } from 'rxjs/operators';
import { ImageRating, SearchConfigService } from './search-config.service';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { environment } from 'src/environments/environment';

export class TransformedEvent<TResponse, TDestination> {
  http: HttpEvent<TResponse>;
  result: TDestination;
}

export class StatusResponse {
  name: string;
  estimatedCount: number;
  storedCount: number;
  indexedCount: number;
  isComplete: boolean;
}

export class StatusResult {
  name: string;
  estimatedCount: number;
  storedCount: number;
  indexedCount: number;
  isComplete: boolean;
  scrapedPercentage: number;
  indexedPercentage: number;
}

export class StatusEvent extends TransformedEvent<StatusResponse[], StatusResult[]> {
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  constructor(private http: HttpClient, private searchConfig: SearchConfigService, private sanitizationService: DomSanitizer) {
  }

  status(): Observable<StatusEvent> {
    let subject = new Subject<StatusEvent>();
    let statusEvent = new StatusEvent();

    this.http.get<StatusResponse[]>(ApiEndpoints.Status, {
      observe: 'events',
    }).subscribe(event => {
      if (event.type === HttpEventType.Response && event.ok) {
        statusEvent.result = event.body.map(r => {
          return {
            name: r.name,
            estimatedCount: r.estimatedCount,
            storedCount: r.storedCount,
            indexedCount: r.indexedCount,
            isComplete: r.isComplete,
            scrapedPercentage: r.isComplete ? 100 : Math.round(r.storedCount / r.estimatedCount * 100),
            indexedPercentage: Math.round(r.indexedCount / (r.isComplete ? r.storedCount : r.estimatedCount) * 100)
          }
        })
      }

      statusEvent.http = event;
      subject.next(statusEvent);
    });

    return subject.asObservable();
  }

  search(croppedImage: Blob, image: Blob): Observable<SearchEvent> {
    let includeNsfw = this.searchConfig.imageRating !== ImageRating.Safe;

    let formData = new FormData();
    formData.append('image', croppedImage);
    formData.append('limit', String(50));
    formData.append('includeNsfw', String(includeNsfw));
    let subject = new Subject<SearchEvent>();
    let searchEvent = new SearchEvent();

    this.http.post<SearchResponse>(ApiEndpoints.Search, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      timeout(15000)
    ).subscribe(event => {
      if (event.type === HttpEventType.Response && event.ok) {
        searchEvent.result = {
          count: event.body.stats.count,
          elapsedMilliseconds: event.body.stats.elapsedMilliseconds,
          imageUrl: this.sanitizationService.bypassSecurityTrustResourceUrl(URL.createObjectURL(image)),
          searchParameters: {
            includeNsfw: includeNsfw
          },
          images: event.body.results.map(i => {
            i.scoreFixed = i.score.toFixed(2);

            if (i.score >= 92) {
              i.match = SearchResultImageMatch.Excellent;
            } else if (i.score > 85) {
              i.match = SearchResultImageMatch.Doubtful;
            } else {
              i.match = SearchResultImageMatch.Unlikely;
            }

            i.credits = i.credits.sort((c1, c2) => {
              if (c1.role == c2.role) {
                return 0;
              }

              return -1;
            });

            i.creditsString = i.credits.map(c => c.name).join(" & ");

            return i;
          })
        }
      }

      searchEvent.http = event;
      subject.next(searchEvent);
    }, error => {
      subject.error(error);
    });

    return subject.asObservable();
  }
}

export interface SearchResultStats {
  count: number;
  elapsedMilliseconds: number;
}

export interface SearchResponse {
  stats: SearchResultStats;
  results: SearchResultImage[];
}

export class SearchResponseImage {
  id: number;
  platform: string;
  viewLocation: string;
  score: number;
  isSfw: boolean;
  thumbnail: SearchResponseThumbnail;
  credits: CreditResponseModel[];
}

export class SearchResultImage extends SearchResponseImage {
  scoreFixed: string;
  match: string;
  creditsString: string;
}

export class SearchResponseThumbnail {
  width: number;
  centerX: number;
  height: number;
  centerY: number;
  location: string;
}

export class CreditResponseModel {
  name: string;
  role: string;
}

export const CreditType = {
  Artist: "artist",
  Submitter: "submitter"
}

export const SearchResultImageMatch = {
  Excellent: "EXCELLENT",
  Doubtful: "DOUBTFUL",
  Unlikely: "UNLIKELY"
}

export class SearchResult {
  imageUrl: SafeResourceUrl;
  count: number;
  elapsedMilliseconds: number;
  images: SearchResultImage[];
  searchParameters: SearchParameters;
}

export class SearchParameters {
  includeNsfw: boolean;
}

export class SearchEvent extends TransformedEvent<SearchResponse, SearchResult> {
}

export class ApiEndpoints {
  static readonly Search: string = ApiEndpoints.GetV1Url('search');
  static readonly Status: string = ApiEndpoints.GetV1Url('status');

  private static GetV1Url(relativeLocation: string): string {
    return environment.baseUrl + '/v1/' + relativeLocation;
  }
}
