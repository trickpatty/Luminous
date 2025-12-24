import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { InvitationService } from './invitation.service';
import { InvitationStatus } from '../../models';

describe('InvitationService', () => {
  let service: InvitationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(InvitationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have initial empty invitations state', () => {
    expect(service.invitations()).toEqual([]);
  });

  it('should have initial false loading state', () => {
    expect(service.loading()).toBeFalse();
  });

  it('should return correct status display for Pending', () => {
    const display = service.getStatusDisplay(InvitationStatus.Pending);
    expect(display.label).toBe('Pending');
    expect(display.color).toContain('yellow');
  });

  it('should return correct status display for Accepted', () => {
    const display = service.getStatusDisplay(InvitationStatus.Accepted);
    expect(display.label).toBe('Accepted');
    expect(display.color).toContain('green');
  });

  it('should return correct status display for Declined', () => {
    const display = service.getStatusDisplay(InvitationStatus.Declined);
    expect(display.label).toBe('Declined');
    expect(display.color).toContain('red');
  });

  it('should clear state', () => {
    service.clearState();
    expect(service.invitations()).toEqual([]);
    expect(service.loading()).toBeFalse();
    expect(service.error()).toBeNull();
  });
});
