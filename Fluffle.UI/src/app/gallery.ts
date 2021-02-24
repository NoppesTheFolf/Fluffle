export class Gallery<TSrc> {
    images: GalleryImage<TSrc>[] = [];

    constructor(private _targetHeight: number, private _maximumHeight: number) {
    }

    set targetHeight(targetHeight: number) {
        this._targetHeight = targetHeight;
    }

    public addImage(width: number, height: number, data: any = null) {
        this.images.push(new GalleryImage(width, height, data));
    }

    public clear() {
        this.images = [];
    }

    public render(targetWidth: number, targetMargin: number, shouldFit: boolean = true): GalleryRow<TSrc>[] {
        let rows: GalleryRow<TSrc>[] = [];

        let row = new GalleryRow<TSrc>(targetWidth, this._targetHeight, this._maximumHeight, targetMargin)
        for (let image of this.images) {
            if (!row.tryAddImage(image)) {
                rows.push(row);

                row = new GalleryRow(targetWidth, this._targetHeight, this._maximumHeight, targetMargin);
                row.tryAddImage(image);
            }
        }

        // Add the final row if it isn't empty
        if (row.any()) {
            rows.push(row);
        }

        if (shouldFit) {
            for (let row of rows) {
                row.fit();
            }
        }

        return rows;
    }
}

export class GalleryRow<TSrc> {
    public images: GalleryRowImage<TSrc>[] = [];

    constructor(private _targetWidth: number, private _targetHeight: number, private _maximumHeight: number, private _targetMargin: number) {
    }

    public tryAddImage(image: GalleryImage<TSrc>): boolean {
        if (!this.hasSpaceFor(image)) {
            return false;
        }

        this.images.push(new GalleryRowImage(this.calculateTheoreticalSpaceTaken(image), this._targetHeight, image.aspectRatio, image.data));

        return true;
    }

    public calculateTheoreticalSpaceTakenIncludingMargin(image: GalleryImage<TSrc>): number {
        let imageWidth = this.calculateTheoreticalSpaceTaken(image);
        let spaceTakenByImageWithMargin = imageWidth + (this._targetMargin * 2);

        if (spaceTakenByImageWithMargin > this._targetWidth) {
            return this._targetWidth;
        }

        return spaceTakenByImageWithMargin;
    }

    public calculateTheoreticalSpaceTaken(image: GalleryImage<TSrc>): number {
        return Math.round(image.aspectRatio * this._targetHeight);
    }

    public any(): boolean {
        return this.images.length > 0;
    }

    public hasSpaceFor(image: GalleryImage<TSrc>): boolean {
        let spaceTaken = this.calculateSpaceTaken();
        let spaceTakenByImage = this.calculateTheoreticalSpaceTakenIncludingMargin(image);
        let newWidth = spaceTaken + spaceTakenByImage;

        return newWidth <= this._targetWidth
    }

    public calculateSpaceLeft(): number {
        let spaceTakenInRow = this.calculateSpaceTaken();

        return this._targetWidth - spaceTakenInRow;
    }

    public fit() {
        let spaceLeft = this.calculateSpaceLeft();

        let totalAspectRatio = 0;
        for (let image of this.images) {
            totalAspectRatio += image.aspectRatio;
        }

        for (let image of this.images) {
            let fractionOfTotalAspectRatio = image.aspectRatio / totalAspectRatio;
            let extraWidth = fractionOfTotalAspectRatio * spaceLeft;
            let widthIncreaseFactor = extraWidth / image.width;

            extraWidth = Math.floor(extraWidth)
            let extraHeight = Math.floor(widthIncreaseFactor * image.height);

            if (image.height + extraHeight >= this._maximumHeight) {
                this.fitMaximumHeight();
                break;
            }

            image.width += extraWidth;
            image.height += extraHeight;
        }
    }

    private fitMaximumHeight() {
        let growFactor = this._maximumHeight / this._targetHeight;

        for (let image of this.images) {
            image.width = Math.floor(image.width * growFactor);
            image.height = this._maximumHeight;
        }
    }

    public calculateSpaceTaken(): number {
        let width = 0;

        for (let image of this.images) {
            width += image.width;
        }

        if (this.images.length > 1) {
            // The two images on the side don't require any margin from its parent. We need to remove that
            let margin = (this.images.length * 2 - 2) * this._targetMargin;

            width += margin
        }

        return width;
    }
}

export class GalleryImage<TSrc> {
    constructor(public width: number, public height: number, public data: TSrc) {
    }

    get aspectRatio(): number {
        return this.width / this.height;
    }
}

export class GalleryRowImage<TSrc> {
    constructor(public width: number, public height: number, public aspectRatio: number, public data: TSrc) {
    }
}
