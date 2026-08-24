export type UserRole = 'Customer' | 'Courier' | 'Admin';
export type ShipmentType = 'Letter' | 'Parcel';
export type ServiceLevel = 'Economy' | 'Standard' | 'Express';
export type ShipmentStatus = 'Created' | 'AwaitingPickup' | 'Accepted' | 'InTransit' | 'OutForDelivery' | 'DeliveryFailed' | 'Delivered' | 'ReturnInitiated' | 'ReturningToSender' | 'ReturnedToSender' | 'Cancelled';
export type DeliveryResult = 'Delivered' | 'RecipientUnavailable' | 'AddressNotFound' | 'Refused' | 'Damaged' | 'Other';

export interface UserProfile { id: string; email: string; displayName: string; role: UserRole }
export interface AuthResponse { accessToken: string; expiresAtUtc: string; user: UserProfile }
export interface Address { id: string; label: string; line1: string; city: string; province: string; postalCode: string; isDefault: boolean }
export interface AddressInput { label: string; line1: string; city: string; province: string; postalCode: string; isDefault: boolean }
export interface AddressView { label: string; line1: string; city: string; province: string; postalCode: string; country: string }
export interface TrackingEvent { status: ShipmentStatus; description: string; occurredAtUtc: string; location?: string }
export interface ShipmentSummary { id: string; trackingCode: string; recipientName: string; destinationCity: string; type: ShipmentType; serviceLevel: ServiceLevel; calculatedPrice: number; status: ShipmentStatus; createdAtUtc: string; courierUserId?: string }
export interface ShipmentDetail extends ShipmentSummary { senderUserId: string; recipientPhone: string; senderAddress: AddressView; destinationAddress: AddressView; weightGrams: number; dimensions?: { lengthCm: number; widthCm: number; heightCm: number }; updatedAtUtc: string; deliveryResult?: DeliveryResult; deliveryNote?: string; history: TrackingEvent[] }
export interface PublicTracking { trackingCode: string; recipient: string; destination: string; type: ShipmentType; serviceLevel: ServiceLevel; status: ShipmentStatus; createdAtUtc: string; history: TrackingEvent[] }
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }
export interface OperationsOverview { totalShipments: number; awaitingPickup: number; inTransit: number; outForDelivery: number; delivered: number; deliveryFailed: number; returning: number; totalRevenue: number }
export interface UserSummary { id: string; email: string; displayName: string; role: UserRole; isActive: boolean }
export interface ProblemDetails { title?: string; detail?: string; errors?: Record<string, string[]>; traceId?: string }
