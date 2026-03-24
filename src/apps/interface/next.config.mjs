/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    remotePatterns: [
      new URL(
        "https://static.wikia.nocookie.net/ffxiv_gamepedia/images/e/e6/**",
      ),
      new URL("https://oldschool.runescape.wiki/images/**"),
      new URL("https://cdn-icons-png.flaticon.com/512/6410/**"),
    ],
  },
};

export default nextConfig;
