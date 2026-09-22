import { Component, ElementRef, computed, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';

interface GeoPoint {
  lat: number;
  lng: number;
  accuracy: number;
}

interface ServiceVisit {
  id: number;
  propertyName: string;
  unit: string;
  serviceType: string;
  timestamp: Date;
  location: GeoPoint;
  photoDataUrl: string;
  violationNote: string | null;
}

const PROPERTIES = [
  { id: 1, name: 'Maple Ridge Apartments', unit: 'Unit 204' },
  { id: 2, name: 'Oakwood Commons', unit: 'Unit 12B' },
  { id: 3, name: 'Riverside Townhomes', unit: 'Unit 7' }
];

const SERVICE_TYPES = ['Trash removal', 'Landscaping', 'Unit inspection', 'Maintenance repair', 'Move-out inspection'];

@Component({
  selector: 'app-field-service-demo',
  standalone: true,
  imports: [FormsModule, DatePipe, DecimalPipe],
  templateUrl: './field-service-demo.html'
})
export class FieldServiceDemoComponent {
  private readonly fileInput = viewChild<ElementRef<HTMLInputElement>>('fileInput');

  readonly properties = PROPERTIES;
  readonly serviceTypes = SERVICE_TYPES;

  readonly selectedPropertyId = signal<number | null>(null);
  readonly selectedServiceType = signal<string | null>(null);

  readonly locating = signal(false);
  readonly location = signal<GeoPoint | null>(null);
  readonly locationError = signal<string | null>(null);

  readonly photoDataUrl = signal<string | null>(null);
  readonly violationFlagged = signal(false);
  readonly violationNote = signal('');

  readonly visits = signal<ServiceVisit[]>([]);

  readonly canSubmit = computed(() =>
    this.selectedPropertyId() !== null &&
    this.selectedServiceType() !== null &&
    this.location() !== null &&
    this.photoDataUrl() !== null
  );

  captureLocation(): void {
    if (!navigator.geolocation) {
      this.locationError.set('Geolocation is not supported in this browser.');
      return;
    }
    this.locating.set(true);
    this.locationError.set(null);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.location.set({ lat: pos.coords.latitude, lng: pos.coords.longitude, accuracy: pos.coords.accuracy });
        this.locating.set(false);
      },
      (err) => {
        this.locationError.set(err.message || 'Could not read location.');
        this.locating.set(false);
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  }

  triggerPhotoPicker(): void {
    this.fileInput()?.nativeElement.click();
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => this.photoDataUrl.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  clearPhoto(): void {
    this.photoDataUrl.set(null);
    const input = this.fileInput()?.nativeElement;
    if (input) input.value = '';
  }

  mapUrl(loc: GeoPoint): string {
    return `https://www.google.com/maps?q=${loc.lat},${loc.lng}`;
  }

  submitVisit(): void {
    const property = this.properties.find(p => p.id === this.selectedPropertyId());
    const serviceType = this.selectedServiceType();
    const loc = this.location();
    const photo = this.photoDataUrl();
    if (!property || !serviceType || !loc || !photo) return;

    this.visits.update(v => [{
      id: Date.now(),
      propertyName: property.name,
      unit: property.unit,
      serviceType,
      timestamp: new Date(),
      location: loc,
      photoDataUrl: photo,
      violationNote: this.violationFlagged() ? (this.violationNote().trim() || 'Flagged — no note provided') : null
    }, ...v]);

    this.selectedServiceType.set(null);
    this.location.set(null);
    this.locationError.set(null);
    this.clearPhoto();
    this.violationFlagged.set(false);
    this.violationNote.set('');
  }
}
