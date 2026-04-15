export type DataPoint = {
  price: number;
  volume: number;
  date: Date;
  minPrice?: number;
  maxPrice?: number;
  openPrice?: number;
  closePrice?: number;
};
