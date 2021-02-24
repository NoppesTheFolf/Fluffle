import { Component, OnInit } from '@angular/core';
import { SearchResultGalleryComponent } from '../search-result-gallery.component';
import { SearchResultService } from '../search-result.service';

@Component({
  selector: 'app-search-result-desktop-gallery',
  templateUrl: './search-result-desktop-gallery.component.html',
  styleUrls: ['./search-result-desktop-gallery.component.scss']
})
export class SearchResultDesktopGalleryComponent extends SearchResultGalleryComponent implements OnInit {
  constructor(searchService: SearchResultService) {
    super(searchService);
  }

  ngOnInit(): void {
    super.ngOnInit();
  }
}
