import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { FileListComponent } from './file-list.component';

describe('FileListComponent', () => {
  let component: FileListComponent;
  let fixture: ComponentFixture<FileListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileListComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(FileListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
