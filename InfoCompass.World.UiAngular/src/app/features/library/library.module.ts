import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LibraryRoutingModule } from './library-routing.module';
import { SharedModule } from 'src/app/shared/shared.module';
import { BookListComponent } from './book-list/book-list.component';

@NgModule({
    imports: [
        CommonModule,
        LibraryRoutingModule,
        SharedModule
    ],
    declarations: [
        BookListComponent
    ]
})
export class LibraryModule { }
