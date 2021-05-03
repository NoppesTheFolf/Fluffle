import { Component, OnDestroy, OnInit } from '@angular/core';
import { ApiService, StatusResult } from 'src/app/api.service';
import { PlatformHelperService } from 'src/app/platform-helper.service';

@Component({
  selector: 'app-status',
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.scss']
})
export class StatusComponent implements OnInit, OnDestroy {
  results: StatusResult[];
  interval: number;

  constructor(private api: ApiService, public platformHelper: PlatformHelperService) {
    this.refreshStatus();

    this.interval = window.setInterval(() => {
      this.refreshStatus();
    }, 5 * 60 * 1000);
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
