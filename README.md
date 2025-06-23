# Leagify Auction Drafter
An auction draft webapp for Leagify, specifically the NFL Draft League
The goal is for this repository will be a Blazor WASM (Webassembly) C# SignalR web application.





## General design:
+ Users can log in as one of several roles. A user can have multiple roles. Here are the roles that exist, and a general description:
  - Auction master. They can create and manage an auction for a league.
  - Team coach. They can bid for a school for their team and assign it to a position on their roster
  - Auction viewer. They can view the auction.
  - Proxy coach. They can bid on behalf of a a team coach who is not present for the auction but would like to have a team.
+ It would be nice if you could log in using Google Auth to make an account.

+ Team coaches pick from a "draft board".
+ At this time, the draft board loads information from a CSV draft template file uploaded by the auction master.  The format of the draft template is similar to the template at the root of this repo, named "SampleDraftTemplate.csv"
+ The Draft Template contains several fields. I will explain them here.
  - School : The school that the draft prospects come from. The thing that will eventually score points.
  - Conference : The conference that the school plays in. For example, Wisconsin (School) plays in the Big Ten (Conference), and LSU (School) plays in the SEC (Conference).
  - ProjectedPoints : This is a projection of how many points this school may score this year. The accuracy of this number varies from year to year.
  - NumberOfProspects : The number of potential draft prospects that have appeared on draft prospect boards 
  - SchoolURL : A URL representing an SVG image that represents the school. The goal 
  - SuggestedAuctionValue : This is supposed to represent a suggested auction value. It is not filled at this point, but might be at some point in the future.
  - LeagifyPosition : Some conferences are large enough to be their own "position" in the draft.Not every conference is large enough to merit its own position, however, so some conferences are but into larger bins, like "RandomSmallSchool". The Notre Dame school doesn't have a conference, so we've elected them to go directly into the "Flex" position instead of a traditional position.
  - ProjectedPointsAboveAverage : This is a calculated number representing the average value of points that a member of this school's conference is projected to have in the draft.
  - ProjectedPointsAboveReplacement : This is a calculated value between this school's projected value compared to the "Replacement Value" of a school from its position.
  - AveragePointsForPosition : This is the average number of points a school in this position
  - ReplacementValueAverageForPosition : The "Replacement Value" of a position is the value of the schools that are left after all of the top schools are presumably selected by the players. For example, if 6 schools from the "ACC" position are projected to be selected in the auction, this is the value of the 7th ACC school, meaning the school you could potentially pick after everyone else has chosen a school.
+ Items up for auction will have several main properties (similar to Player and Position)
+ Each bidder will be trying to fill a "roster" of "positions"
+ Each bidder will have a budget, which is editable before the auction begins, but not afterwards.
+ Bidders agree to join an "auction" and the auction order is set up in a normal order.
+ The auction includes a known list of potential "players" (no values are pre-assigned, at this point)
+ Each bidder can view:
  - The remaining "players" available
  - The "player" up for bid
  - The current bid
  - Current highest bidder
  - The bidder's "roster"
  - The bidder's available money


## The Auction process
+ At the beginning of the auction, each team coach has a budget, which is usually the same, like 200 dollars, unless the auction master determines that one team should have a seperate budget because they arrived late or need to have a handicap. 
+ A group of team coaches will be bidding on players.
+ Each bidder can put a "School" up for bid.
+ Bidding continues until all other team coaches pass or no other team coach can afford to bid (determined by remaining spots in roster multiplied by minimum bid amount).
+ After bidding completes, the "School" is placed on the bidders roster, and the amount is deducted from the bidder's budget.
+ If a bidder's roster is full, they can no longer nominate players for bid, and they can no longer win auctions for schools.
+ Bidding continues until each team coach's roster is full.
+ At the completion of the auction, a CSV can be downloaded, which contains:
  - The bidder name
  - The school name
  - The conference position that the bidder put the school into
  - The auction cost
  - A variety of other school information that comes directly from the uploaded CSV
  - An example of the desired output can be found in this repository with the name "SampleFantasyDraft.csv".


# The Leagify NFL Draft Game
+ The goal is to have the most points at the end of the draft.
+ “Team coaches” bid on schools 
+ Bonus points are provided for picks that are the result of a trade, which adds a little variance to the game. The highest scoring coach at the end of the NFL Draft is the winner. This game runs once per year, corresponding to the NFL draft.
+ Each school is a part of a Conference. Generally, those conferences correspond to positions, but there are also "Flex" positions that accomodate several different conference designations.
+ Schools have athletes. Those athletes are drafted in the NFL Draft. Those draft picks correspond to a value chart created by Leagify.


Nice to haves:

+ It would be nice for the "Auction Master" could save the auction CSV result file somewhere on the server for later viewing
+ We could color code the "positions" via CSS or something.

Technologies:
+ C#
+ SignalR
+ Blazor WASM

