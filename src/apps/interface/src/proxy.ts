import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Proxy scaffold - add auth guards, rewrites, CSP headers, etc. here.
export function proxy(request: NextRequest) {
  return NextResponse.next();
}

export const config = {
  matcher: [
    // Skip internal Next.js paths and static assets
    "/((?!_next/static|_next/image|favicon.ico|.*\\.svg$).*)",
  ],
};
