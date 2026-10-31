const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const SAFE_PATH_PATTERN = /^(\/[\w-]+)+$/;

export const ORIGIN_PARAM = "from";

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

function pathTrail(pathname: string): BreadcrumbCrumb[] {
  const segments = pathname.split("/").filter(Boolean);
  return segments.map((segment, index) => ({
    href: `/${segments.slice(0, index + 1).join("/")}`,
    segment,
    label: GUID_PATTERN.test(segment) ? undefined : titleCase(segment),
  }));
}

// A detail page reached from a list (e.g. Explorer -> symbol) shows that list before itself.
// Only paths below the page's own parent count, so a crafted origin can't add unrelated crumbs.
function isOriginOf(pathname: string, origin: string): boolean {
  const parent = pathname.slice(0, pathname.lastIndexOf("/"));
  return SAFE_PATH_PATTERN.test(origin) && origin !== pathname && origin.startsWith(`${parent}/`);
}

export function buildBreadcrumbTrail(
  pathname: string,
  origin?: string | null,
  labels: Record<string, string> = {},
): BreadcrumbCrumb[] {
  const trail = pathTrail(pathname);
  const current = trail.at(-1);
  if (!origin || !current || !isOriginOf(pathname, origin)) {
    return trail;
  }

  // After a reload nothing has registered the origin's own entities (e.g. a group), so fall back
  // to the path trail rather than show crumbs that would never resolve.
  const originTrail = pathTrail(origin);
  const isUnresolvable = originTrail.some(
    (crumb) =>
      crumb.label === undefined &&
      labels[crumb.segment] === undefined &&
      !trail.some((own) => own.segment === crumb.segment),
  );
  return isUnresolvable ? trail : [...originTrail, current];
}
