import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { FileFormComponent } from './file-form.component';

describe('FileFormComponent', () => {
  let component: FileFormComponent;
  let fixture: ComponentFixture<FileFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileFormComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(FileFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
