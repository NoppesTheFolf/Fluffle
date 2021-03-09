import { Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService, StatusResult } from 'src/app/api.service';
import { ImageHelper } from 'src/app/search-result-gallery.component';

@Component({
  selector: 'app-status',
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.scss']
})
export class StatusComponent implements OnInit, OnDestroy {
  ImageHelper = ImageHelper;

  results: StatusResult[];
  interval: number;

  constructor(private api: ApiService) {
    this.refreshStatus();

    this.interval = window.setInterval(() => {
      this.refreshStatus();
    }, 5000);
  }

  ngOnInit(): void {
  }

  refreshStatus(): void {
    this.api.status().subscribe(event => {
      this.results = event.result;
    });
  }

  ngOnDestroy(): void {
    window.clearInterval(this.interval);
  }
}
