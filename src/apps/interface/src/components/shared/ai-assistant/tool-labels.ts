export function toolLabel(name: string, labels?: Record<string, string>): string {
  return labels?.[name] ?? name.replace(/^(get|find)_/, "").replaceAll("_", " ");
}
