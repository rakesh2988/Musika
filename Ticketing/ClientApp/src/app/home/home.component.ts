import { Component, OnInit } from '@angular/core';
import { RVCellService } from '../service/service';
import { debug } from 'util';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit{
  constructor(private RvCellService: RVCellService, private router: Router) {
    //localStorage.removeItem('currentUser');
    //localStorage.clear();
    this.router.navigate(['/login']);
  }

  ngOnInit() {
  }
 
}
