import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { RouterLink, RouterOutlet } from '@angular/router';

describe('AppComponent Unit Tests Suite', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;  

  beforeEach(async () => {
    
    await TestBed.configureTestingModule({
      imports: [AppComponent, RouterOutlet, RouterLink],
      providers: [       
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
  });

  it('should create the AppComponent', () => {
    expect(component).toBeTruthy();
  });
});