import { Pipe, PipeTransform, Injectable } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

//@Pipe({ name: 'safeHtml' })
//export class SafeHtml {
//  constructor(private sanitizer: DomSanitizer) { }

//  transform(html) {
//    return this.sanitizer.bypassSecurityTrustResourceUrl(html);
//  }
//}




@Pipe({ name: 'filter'})
@Injectable()
export class FilterPipe implements PipeTransform {
  transform(items: any[], field: string, value: string): any[] {
    if (!items) {
      return [];
    }
    if (!field || !value) {
      return items;
    }

    return items.filter(singleItem =>
      singleItem[field].toLowerCase().includes(value.toLowerCase())
    );
  }
}
