import { InjectionToken } from '@angular/core';

// I honestly have no idea where to put this. In the AppModule will result in circular dependencies.
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');
