import { Component, OnInit } from '@angular/core';
import { SearchResultImage } from '../api.service';
import { ImageHelper, SearchResultGalleryComponent } from '../search-result-gallery.component';
import { SearchResultService } from '../search-result.service';

@Component({
  selector: 'app-search-result-mobile',
  templateUrl: './search-result-mobile.component.html',
  styleUrls: ['./search-result-mobile.component.scss']
})
export class SearchResultMobileComponent extends SearchResultGalleryComponent implements OnInit {
  ImageHelper;

  images: SearchResultImage[];

  constructor(searchService: SearchResultService) {
    super(searchService);
    
    this.ImageHelper = ImageHelper;

    this.images = searchService.result.images.slice(0, 12);
  }

  ngOnInit(): void {
    super.ngOnInit();
  }
}
