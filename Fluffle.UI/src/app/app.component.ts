import { AfterViewInit, Component, HostListener } from '@angular/core';
import { NavigationStart, Router } from '@angular/router';
import { ContentCenterService } from './content-center.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements AfterViewInit {
  centerContent = true;
  title = 'Fluffle';

  constructor(router: Router, public contentCenterService: ContentCenterService) {
    contentCenterService.onCenter.subscribe(event => {
      this.centerContent = event;
    });

    router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        this.centerContent = false;
      }
    });
  }

  ngAfterViewInit(): void {
    this.onResize();
  }

  @HostListener('window:resize')
  onResize() {
    // This is needed for Chrome its annoying URL bar
    document.body.style.minHeight = `${window.innerHeight}px`;

    let mobileNavbar = document.getElementById("mobile-navbar");
    document.getElementById("mobile-navbar-dummy").style.height = `${mobileNavbar.clientHeight}px`;
  }
}
