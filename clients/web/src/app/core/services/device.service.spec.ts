import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { DeviceService } from './device.service';
import { DeviceType } from '../../models';

describe('DeviceService', () => {
  let service: DeviceService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DeviceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have initial empty devices state', () => {
    expect(service.devices()).toEqual([]);
  });

  it('should have initial false loading state', () => {
    expect(service.loading()).toBeFalse();
  });

  it('should have initial null link code state', () => {
    expect(service.linkCode()).toBeNull();
  });

  it('should return correct device type display names', () => {
    expect(service.getDeviceTypeDisplayName(DeviceType.Display)).toBe('Wall Display');
    expect(service.getDeviceTypeDisplayName(DeviceType.Mobile)).toBe('Mobile App');
    expect(service.getDeviceTypeDisplayName(DeviceType.Web)).toBe('Web Browser');
  });

  it('should return correct device type icons', () => {
    expect(service.getDeviceTypeIcon(DeviceType.Display)).toBe('display');
    expect(service.getDeviceTypeIcon(DeviceType.Mobile)).toBe('smartphone');
    expect(service.getDeviceTypeIcon(DeviceType.Web)).toBe('globe');
  });

  it('should clear link code', () => {
    service.clearLinkCode();
    expect(service.linkCode()).toBeNull();
  });

  it('should clear state', () => {
    service.clearState();
    expect(service.devices()).toEqual([]);
    expect(service.loading()).toBeFalse();
    expect(service.error()).toBeNull();
    expect(service.linkCode()).toBeNull();
  });
});
