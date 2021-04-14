import { AfterViewChecked, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ContentCenterService } from 'src/app/content-center.service';
import { ImageRating, SearchConfigService } from 'src/app/search-config.service';
import { SearchResultService, SearchState } from 'src/app/search-result.service';
import { TitleService } from 'src/app/title.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss'],
  providers: [SearchResultService]
})
export class SearchComponent implements OnInit, AfterViewChecked {
  @ViewChild('container') container: ElementRef<HTMLElement>;
  @ViewChild('canvas') canvas: ElementRef<HTMLCanvasElement>;
  environment = environment;
  ImageRating = ImageRating;
  SearchState = SearchState;
  previousState: SearchState;

  constructor(titleService: TitleService, private contentCenterService: ContentCenterService,
    public searchResultService: SearchResultService, public config: SearchConfigService) {
    titleService.title = "Reverse search";
    this.contentCenterService.center();
  }

  ngOnInit(): void {
  }

  ngAfterViewChecked(): void {
    if (this.searchResultService.state === SearchState.Finished && this.searchResultService.state !== this.previousState) {
      setTimeout(() => {
        this.container.nativeElement.scrollIntoView({
          behavior: 'smooth'
        });
      }, 250);
    }

    this.previousState = this.searchResultService.state;
  }

  onDragover(event: DragEvent) {
    event.preventDefault();
  }

  onSelect(event) {
    this.search(event.target.files);
  }

  onDrop(event: DragEvent) {
    // Prevent file from being opened
    event.preventDefault();
    this.search(event.dataTransfer.files);
  }

  calculateThumbnailSize(width: number, height: number, target: number): [number, number] {
    let determineSize = (sizeOne: number, sizeTwo: number, sizeOneTarget: number): number => {
      var aspectRatio = sizeOneTarget / sizeOne;

      return Math.round(aspectRatio * sizeTwo);
    };

    if (width == height) {
      return [target, target];
    }

    return width > height
      ? [determineSize(height, width, target), target]
      : [target, determineSize(width, height, target)];
  }

  search(files: FileList) {
    if (files.length > 1) {
      this.searchResultService.setError('You can only reverse search a single image each time at the moment.');
      return;
    }
    let file = files[0];

    this.searchResultService.state = SearchState.Preparing;

    const image = new Image();
    image.onload = () => {
      const target = 256;
      let thumbnailSize = this.calculateThumbnailSize(image.width, image.height, target);

      // In the first place we scaled down the image to a fixed size (250x250), but that
      // caused such a significant loss in image quality in some instances that we had to
      // scale preserving the aspect ratio of the original image  
      this.canvas.nativeElement.width = thumbnailSize[0];
      this.canvas.nativeElement.height = thumbnailSize[1];
      const ctx = this.canvas.nativeElement.getContext('2d');
      ctx.drawImage(image, 0, 0, this.canvas.nativeElement.width, this.canvas.nativeElement.height);

      let dataUri = this.canvas.nativeElement.toDataURL("image/png");
      let base64EncodedData = dataUri.split(',')[1];
      let data = atob(base64EncodedData);
      var array = new Uint8Array(data.length);
      for (let i = 0; i < data.length; i++) {
        array[i] = data.charCodeAt(i);
      }
      let croppedImage = new Blob([array]);
      this.searchResultService.search(croppedImage, file);
    };

    // The error might simply be that the image format isn't supported by the canvas.
    // Therefore, we should still send it to the server.
    image.onerror = () => {
      this.searchResultService.search(file, file);
    }

    image.src = URL.createObjectURL(file);
  }
}
