import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { UserService } from './user.service';
import { UserRole } from '../../models';

describe('UserService', () => {
  let service: UserService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(UserService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have initial empty members state', () => {
    expect(service.members()).toEqual([]);
  });

  it('should have initial false loading state', () => {
    expect(service.loading()).toBeFalse();
  });

  it('should return correct role display names', () => {
    expect(service.getRoleDisplayName(UserRole.Owner)).toBe('Owner');
    expect(service.getRoleDisplayName(UserRole.Admin)).toBe('Admin');
    expect(service.getRoleDisplayName(UserRole.Adult)).toBe('Adult');
    expect(service.getRoleDisplayName(UserRole.Teen)).toBe('Teen');
    expect(service.getRoleDisplayName(UserRole.Child)).toBe('Child');
    expect(service.getRoleDisplayName(UserRole.Caregiver)).toBe('Caregiver');
  });

  it('should return assignable roles for Owner', () => {
    const roles = service.getAssignableRoles(UserRole.Owner);
    expect(roles).toContain(UserRole.Admin);
    expect(roles).toContain(UserRole.Adult);
    expect(roles).not.toContain(UserRole.Owner);
  });

  it('should return assignable roles for Admin', () => {
    const roles = service.getAssignableRoles(UserRole.Admin);
    expect(roles).toContain(UserRole.Adult);
    expect(roles).not.toContain(UserRole.Admin);
    expect(roles).not.toContain(UserRole.Owner);
  });

  it('should clear state', () => {
    service.clearState();
    expect(service.members()).toEqual([]);
    expect(service.loading()).toBeFalse();
    expect(service.error()).toBeNull();
  });
});
