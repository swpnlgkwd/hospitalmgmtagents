import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShiftCalendarComponent } from './shift-calendar.component';

describe('ShiftCalendarComponent', () => {
  let component: ShiftCalendarComponent;
  let fixture: ComponentFixture<ShiftCalendarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShiftCalendarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShiftCalendarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
