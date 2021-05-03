import { Injectable } from "@angular/core";
import { paramCase } from "param-case";
import { SafeUrl } from "@angular/platform-browser";
import { SvgService } from "./svg.service";
import { SearchResultImage, SearchResultImageMatch } from "./api.service";

@Injectable({
    providedIn: 'root'
})
export class PlatformHelperService {
    constructor(private svg: SvgService) { }

    getLabel(image: SearchResultImage): string {
        if (image.match === SearchResultImageMatch.Excellent) {
            return 'bg-success';
        }

        if (image.match === SearchResultImageMatch.Doubtful) {
            return 'bg-warning';
        }

        return 'bg-danger';
    }

    getLogo(platformName: string): SafeUrl {
        return this.svg.get(paramCase(platformName));
    }
}
