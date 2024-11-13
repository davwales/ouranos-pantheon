export interface AxisConfiguration {
    label: string;
    color: string;
    isActive: boolean;
    isRight: boolean;
    formatter: (x: number | null) => string;
    value: (x: any) => string | number | Date;
};