const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export type BreadcrumbCrumb = {
  href: string;
  segment: string;
  label?: string;
};

function titleCase(segment: string): string {
  return segment
    .split("-")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}

export function buildBreadcrumbTrail(pathname: string): BreadcrumbCrumb[] {
  const segments = pathname.split("/").filter(Boolean);
  return segments.map((segment, index) => ({
    href: `/${segments.slice(0, index + 1).join("/")}`,
    segment,
    label: GUID_PATTERN.test(segment) ? undefined : titleCase(segment),
  }));
}
