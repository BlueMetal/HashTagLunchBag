import { Component, Input, OnInit, AfterViewInit } from '@angular/core';
import { flyingHeart } from '../../utils/custom-animation';

@Component({
    selector: 'heart-icon',
    templateUrl: './heart-icon.component.html',
    animations: [
      flyingHeart
    ]
  })
export class HeartIconComponent implements OnInit, AfterViewInit {
    @Input() moveToDir: string;
    state = 'initialSt';
    animateMe() {
        this.state = (this.state === 'initialSt' ? 'right' : 'initialSt');
      }
     constructor() {
         this.state = 'initialSt';
     }

    ngOnInit() {
       // this.animateMe();
    }

    ngAfterViewInit() {
       setTimeout(() => {
        this.state = this.moveToDir;
       }, 500);
    }
}
