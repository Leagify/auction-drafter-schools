# Leagify Auction Drafter
An auction draft webapp for Leagify, specifically the NFL Draft League
The goal is for this repository will be a Blazor WASM (Webassembly) C# SignalR web application.





## General design:
+ Users can log in as one of several roles. A user can have multiple roles. Here are the roles that exist, and a general description:
  - Auction master. They can create an auction for a league.
  - Team manager. They can bid for a school for their team
  - Auction viewer. They can view the auction.
+ It would be nice if you could log in using Google.

+ Team managers pick from a "draft board".
+ At this time, the draft board loads information from a CSV draft template file uploaded by the auction master.  The format of the draft template is similar to the template at the root of this repo, named "sampleDraftTemplate.csv"
+ The Draft Template contains several fields. I will explain them here.
  - School : 
  - Conference : 
  - ProjectedPoints : 
  - NumberOfProspects : 
  - SchoolURL : 
  - SuggestedAuctionValue :
  - LeagifyPosition :
  - ProjectedPointsAboveAverage : 
  - ProjectedPointsAboveReplacement :
  - AveragePointsForPosition :
  - ReplacementValueAverageForPosition :
+ Items up for auction will have two properties (similar to Player and Position)
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
+ Each bidder can put a "player" up for bid.
+ Bidding continues until all other players pass or no other player can afford to bid (determined by remaining spots in roster multiplied by minimum bid amount).
+ After bidding completes, the "player" is placed on the bidders roster, and the amount is deducted from the bidders budget.
+ If a bidder's roster is full, they can no longer nominate players for bid.
+ Bidding continues until each player's roster is full.
+ At the completion of the auction, a CSV can be downloaded, which contains:
  - The bidder name
  - The player name
  - The position
  - The auction cost

# The Leagify NFL Draft Game
+ The goal is to have the most points at the end of the draft.
+ “Team managers” bid on schools 
+ Bonus points are provided for picks that are the result of a trade, which adds a little variance to the game. The highest scoring coach at the end of the NFL Draft is the winner. This game runs once per year, corresponding to the NFL draft.
+ Each school is
+ Schools have athletes. Those athletes are drafted in the NFL Draft. Those draft picks correspond to a value chart created by Leagify.


Nice to haves:

+ It would be nice for the "Auction Master" could save the auction CSV result file somewhere on the server for later viewing
+ We could color code the "positions"

Technologies:
+ C#
+ SignalR
+ Blazor WASM

