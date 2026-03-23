import InfoCard from "@/app/components/info-card";
import { Typography } from "@/app/components/typography";
import Link from "next/link";

const modules: {
  name: string;
  description: string;
  href: string;
  icon?: string;
}[] = [
  {
    name: "Explorer",
    description:
      "Explore market data and gain insights into pricing trends to make informed investment decisions.",
    href: "/plutus/explorer",
  },
  {
    name: "Recipes",
    description:
      "Discover how you can combine different symbols to exploit a margin.",
    href: "/plutus/recipes",
  },
  {
    name: "Forecasts",
    description: "Get a glimpse of the future with our AI-powered forecasts.",
    href: "/plutus/forecasts",
  },
  {
    name: "Recent Trades",
    description:
      "View trades as they come in, gaining insights into the market before anyone else.",
    href: "/plutus/recent-trades",
  },
  {
    name: "Signal Rankings",
    description:
      "See which symbols have the strongest signals across all investment intents.",
    href: "/plutus/signals",
  },
];

export default function Plutus() {
  return (
    <>
      <Typography variant="h1" className="mb-10">
        Plutus
      </Typography>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {modules.map((module, moduleIndex) => (
          <Link href={module.href} key={moduleIndex}>
            <InfoCard
              label={module.name}
              description={module.description}
              iconSrc={module.icon}
              className="hover:bg-accent h-full w-full"
            />
          </Link>
        ))}
      </div>
    </>
  );
}
