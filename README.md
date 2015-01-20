Bitcoin-Tool
============

Collection of bitcoin related classes, can be used to impliment
a variety of functions.

* Work with addresses, public and private keys
* Generate and sign transactions
* Connect over P2P to a bitcoin node
* Easily generate and evaulate custom scripts
* Decode and encode blocks, transactions, scripts, network messages
* Parse blockchain for data

Program.cs contains test code/me playing around/etc and usually contains
commented out code that can be used for examples.

Apps directory contains mostly complete programs that use the code.

ComputeUnspentTxOut.cs creates and updates a list of all unspent outputs. 
- Requires that the blockchain contain no orphan blocks.

ComputerAddressBalances.cs uses the unspent txout list to compute a balance of all addresses.

Use FindFirstOrphan.cs to find the file containing orphaned blocks, delete that and all subsequent 
block files and resync bitcoind.

MakeBootstrap.cs is a failed attempt at creating a bootstrap.dat blockchain without orphaned blocks.

Code is GPLv3
https://www.gnu.org/licenses/gpl-3.0.html
