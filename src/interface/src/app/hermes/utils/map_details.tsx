export const mapDetails = (details: Array<{ key: string; value: string }>) =>
    details.map(({ key, value }) => ({ key, value }));