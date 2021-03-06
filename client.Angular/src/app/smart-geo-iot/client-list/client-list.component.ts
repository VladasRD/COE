import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { GrudList } from '../../common/grud-list';
import { SmartGeoIotService } from './../smartgeoiot.service';
import { Client } from '../client';

@Component({
  selector: 'app-client-list',
  templateUrl: './client-list.component.html',
  styleUrls: ['./client-list.component.css']
})
export class ClientListComponent extends GrudList<Object> implements OnInit {
  form: FormGroup;
  displayedColumns: string[] = ['name', 'document', 'city_state', 'postalCode', 'active', 'notifications'];

  constructor(private sgiService: SmartGeoIotService) {
    super();
    this.form = new FormGroup({
      'filterStatus': new FormControl('')
    });

    this.form.get('filterStatus').valueChanges.subscribe(val => {
      this.newSearch();
    });

   }

  ngOnInit() {
  }

  private get filterStatus() {
    return this.form.get('filterStatus').value;
  }

  getResults(): Observable<Client[]> {
    return this.sgiService.getClients(this.searchFilter$.getValue(), this.filterStatus, false, this._skip, this._pageSize, c => { this._totalCount = c; });
  }

}
