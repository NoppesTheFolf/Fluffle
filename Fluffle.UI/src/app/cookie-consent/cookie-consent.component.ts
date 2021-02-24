import { Component } from '@angular/core';

@Component({
  selector: 'app-cookie-consent',
  templateUrl: './cookie-consent.component.html',
  styleUrls: ['./cookie-consent.component.scss']
})
export class CookieConsentComponent {
  private key = "consent-given-v1"; // We can simply increase the version to ask for cookies again after an update

  _consentGiven: boolean = false;

  set consentGiven(value: boolean) {
    this._consentGiven = value;
    localStorage.setItem(this.key, String(value));
  }

  get consentGiven(): boolean {
    return this._consentGiven;
  }

  constructor() {
    let consentGiven = localStorage.getItem(this.key);
    if (consentGiven !== null) {
      this.consentGiven = Boolean(consentGiven);
    }
  }
}
