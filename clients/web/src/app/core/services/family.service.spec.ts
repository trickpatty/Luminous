import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { FamilyService } from './family.service';

describe('FamilyService', () => {
  let service: FamilyService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(FamilyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have initial null family state', () => {
    expect(service.family()).toBeNull();
  });

  it('should have initial false loading state', () => {
    expect(service.loading()).toBeFalse();
  });

  it('should have initial null error state', () => {
    expect(service.error()).toBeNull();
  });

  it('should clear state', () => {
    service.clearState();
    expect(service.family()).toBeNull();
    expect(service.loading()).toBeFalse();
    expect(service.error()).toBeNull();
  });
});
