import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavbarComponent } from './navbar/navbar.component';
import { AboutComponent } from './pages/about/about.component';
import { ContactComponent } from './pages/contact/contact.component';
import { SearchComponent } from './pages/search/search.component';
import { ContentCenterService } from './content-center.service';
import { ApiService } from './api.service';
import { SearchConfigService } from './search-config.service';
import { SearchResultDesktopComponent } from './search-result-desktop/search-result-desktop.component';
import { ReloadDirective } from './reload.directive';
import { EmptyComponent } from './pages/empty/empty.component';
import { TitleService } from './title.service';
import { StatusComponent } from './pages/status/status.component';
import { SearchResultMobileComponent } from './search-result-mobile/search-result-mobile.component';
import { SearchResultDesktopHeaderComponent } from './search-result-desktop-header/search-result-desktop-header.component';
import { SearchResultDesktopGalleryComponent } from './search-result-desktop-gallery/search-result-desktop-gallery.component';
import { SearchResultDesktopGalleryImageComponent } from './search-result-desktop-gallery-image/search-result-desktop-gallery-image.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { CookieConsentComponent } from './cookie-consent/cookie-consent.component';
import { NavbarMobileComponent } from './navbar-mobile/navbar-mobile.component';
import { DragClassDirective } from './drop-class.directive';
import { GalleryThumbnailComponent } from './gallery-thumbnail/gallery-thumbnail.component';
import { SvgService } from './svg.service';
import { PlatformHelperService } from './platform-helper.service';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    AboutComponent,
    ContactComponent,
    SearchComponent,
    SearchResultDesktopComponent,
    ReloadDirective,
    EmptyComponent,
    StatusComponent,
    SearchResultMobileComponent,
    SearchResultDesktopHeaderComponent,
    SearchResultDesktopGalleryComponent,
    SearchResultDesktopGalleryImageComponent,
    NotFoundComponent,
    CookieConsentComponent,
    NavbarMobileComponent,
    DragClassDirective,
    GalleryThumbnailComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [
    TitleService,
    ContentCenterService,
    SearchConfigService,
    SvgService,
    PlatformHelperService,
    ApiService
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
