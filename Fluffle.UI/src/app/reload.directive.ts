import { Directive, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Directive({
  selector: '[appReload]'
})
export class ReloadDirective {
  constructor(private router: Router, private route: ActivatedRoute) { }

  @HostListener('click') onClick() {
    let urlTree = this.router.createUrlTree(['.'], { relativeTo: this.route });
    let navigateTo = this.router.serializeUrl(urlTree);

    this.router.navigateByUrl('/empty', { skipLocationChange: true }).then(() => {
      this.router.navigateByUrl(navigateTo);
    });
  }
}
